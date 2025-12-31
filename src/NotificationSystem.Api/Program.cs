using NotificationSystem.Api.Endpoints;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Api.Middlewares;
using NotificationSystem.Application;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Options;
using NotificationSystem.Application.Services;
using NotificationSystem.Infrastructure;
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
    builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
    builder.Services.AddScoped<ISmtpService, SmtpService>();

    // Configure CORS - allow all origins
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure ResultExtensions
    ResultExtensions.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

    // Add global exception handler middleware
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Configure the HTTP request pipeline.
    app.UseSwaggerConfiguration(app.Environment);

    app.UseCors();

    app.UseHttpsRedirection();

    // Map endpoints
    app.MapNotificationEndpoints();

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
