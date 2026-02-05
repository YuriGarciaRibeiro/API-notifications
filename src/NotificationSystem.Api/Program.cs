using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NotificationSystem.Api.Authorization;
using NotificationSystem.Api.Endpoints;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Api.Middlewares;
using NotificationSystem.Application;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Infrastructure;
using NotificationSystem.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting NotificationSystem API");

    // Add services to the container.
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddCustomProblemDetails();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSwaggerConfiguration();

    // Configure SMTP
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
    builder.Services.AddScoped<IEmailService, SmtpService>();

    // Configure RabbitMQ Settings for DLQ Service
    builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

    // Configure Dead Letter Queue Management
    builder.Services.AddSingleton<IDeadLetterQueueService, DeadLetterQueueService>();
    builder.Services.AddHostedService<DeadLetterQueueMonitorService>();

    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["Secret"];

    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("JWT Secret not configured in appsettings.json");
    }

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
        });

    builder.Services.AddAuthorization(options =>
    {
        // Permission policies
        options.AddPolicy("role.create", policy => policy.RequireClaim("permission", "role.create"));
        options.AddPolicy("role.update", policy => policy.RequireClaim("permission", "role.update"));
        options.AddPolicy("role.delete", policy => policy.RequireClaim("permission", "role.delete"));
        options.AddPolicy("role.view", policy => policy.RequireClaim("permission", "role.view"));

        options.AddPolicy("user.create", policy => policy.RequireClaim("permission", "user.create"));
        options.AddPolicy("user.update", policy => policy.RequireClaim("permission", "user.update"));
        options.AddPolicy("user.delete", policy => policy.RequireClaim("permission", "user.delete"));
        options.AddPolicy("user.view", policy => policy.RequireClaim("permission", "user.view"));
    });

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {   
            // aceita qualquer origem
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }

    // Configure ResultExtensions
    ResultExtensions.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

    // Add global exception handler middleware
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Configure the HTTP request pipeline.
    app.UseSwaggerConfiguration(app.Environment);

    app.UseCors();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // Map endpoints
    app.MapNotificationEndpoints();
    app.MapAuthEndpoints();
    app.MapUserEndpoints();
    app.MapRoleEndpoints();
    app.MapDeadLetterQueueEndpoints();
    app.MapProviderEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
