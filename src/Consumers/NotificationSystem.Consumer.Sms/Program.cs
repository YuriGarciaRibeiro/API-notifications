using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Services;
using NotificationSystem.Infrastructure;
using NotificationSystem.Infrastructure.Persistence;
using NotificationSystem.Infrastructure.Persistence.Repositories;
using NotificationSystem.Worker.Sms;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

// Configure Options
builder.Services.Configure<TwilioSettings>(
    builder.Configuration.GetSection(TwilioSettings.SectionName));

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(DatabaseSettings.SectionName));

// Database
builder.Services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
{
    var databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    if (string.IsNullOrEmpty(databaseSettings.ConnectionString))
    {
        throw new InvalidOperationException("Database connection string not found.");
    }

    options.UseNpgsql(
        databaseSettings.ConnectionString,
        b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)
    );
});

// Repositories (only what this consumer needs)
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IProviderConfigurationRepository, ProviderConfigurationRepository>();

// Register error handling middleware and retry strategy
builder.Services.AddSingleton<IRetryStrategy>(sp =>
    new ExponentialBackoffRetryStrategy(
        maxRetries: 3,
        initialDelay: TimeSpan.FromSeconds(2),
        maxDelay: TimeSpan.FromMinutes(5)));
builder.Services.AddSingleton<MessageProcessingMiddleware<SmsChannelMessage>>();

// Register Provider Factories (dynamic provider configuration)
builder.Services.AddProviderFactories();

// Register Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
