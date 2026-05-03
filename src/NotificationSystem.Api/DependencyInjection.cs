using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificationSystem.Api.Endpoints;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Api.Hubs;
using NotificationSystem.Api.Infrastructure.Hangfire;
using NotificationSystem.Api.Middlewares;
using NotificationSystem.Application;
using NotificationSystem.Application.Authorization;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Infrastructure;
using NotificationSystem.Infrastructure.Persistence;
using Serilog;

namespace NotificationSystem.Api;

public static class DependencyInjection
{
    /// <summary>
    /// Configures Serilog logging.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }

    /// <summary>
    /// Configures all application services.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core services
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddCustomProblemDetails();
        services.AddHttpContextAccessor();
        services.AddSwaggerConfiguration();
        services.AddSignalR();

        // Hangfire Configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    SchemaName = "hangfire"
                }));

        // Adicionar servidor Hangfire
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "campaigns" };
            options.ServerName = $"{Environment.MachineName}:hangfire";
        });

        // Configuration-based services
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.AddScoped<IEmailService, SmtpService>();

        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddSingleton<IDeadLetterQueueService, DeadLetterQueueService>();
        services.AddHostedService<DeadLetterQueueMonitorService>();
        services.AddSingleton<IBulkProgressSubscriptionStore, BulkProgressSubscriptionStore>();
        services.AddHostedService<BulkProgressBroadcasterService>();

        return services;
    }


    /// <summary>
    /// Configures JWT authentication.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret not configured in appsettings.json");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/bulk-progress"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    /// <summary>
    /// Configures authorization policies.
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddPermissionPolicies();
        return services;
    }

    /// <summary>
    /// Configures CORS policy.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    /// <summary>
    /// Initializes the database with migrations and seed data.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }

    /// <summary>
    /// Configures the HTTP request pipeline.
    /// </summary>
    public static void ConfigureHttpPipeline(this WebApplication app)
    {
        // Initialize Result extension
        var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
        ResultExtensions.Configure(httpContextAccessor);

        // Exception handling
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        // Documentation
        app.UseSwaggerConfiguration(app.Environment);

        // CORS before authentication
        app.UseCors();

        // Security
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // Hangfire Dashboard
        if (app.Environment.IsDevelopment())
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardNoAuthFilter() }
            });
        }
        else
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
        }

        // Endpoints
        app.MapNotificationEndpoints();
        app.MapBulkNotificationEndpoints();
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapRoleEndpoints();
        app.MapDeadLetterQueueEndpoints();
        app.MapProviderEndpoints();
        app.MapAuditLogEndpoints();
        app.MapHub<BulkProgressHub>("/hubs/bulk-progress")
            .RequireAuthorization(Permissions.BulkNotificationView);
    }
}
