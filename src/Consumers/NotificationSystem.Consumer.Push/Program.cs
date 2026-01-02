using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Options;
using NotificationSystem.Application.Services;
using NotificationSystem.Infrastructure.Persistence;
using NotificationSystem.Infrastructure.Persistence.Repositories;
using NotificationSystem.Infrastructure.Settings;
using NotificationSystem.Worker.Push;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

// Configure Options
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

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

// Register error handling middleware and retry strategy
builder.Services.AddSingleton<IRetryStrategy>(sp =>
    new ExponentialBackoffRetryStrategy(
        maxRetries: 3,
        initialDelay: TimeSpan.FromSeconds(2),
        maxDelay: TimeSpan.FromMinutes(5)));
builder.Services.AddSingleton<MessageProcessingMiddleware<PushChannelMessage>>();

// Initialize Firebase Admin SDK
var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];
if (!string.IsNullOrEmpty(firebaseCredentialsPath) && File.Exists(firebaseCredentialsPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseCredentialsPath)
    });
}
else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
}
else
{
    Log.Warning("Firebase credentials not configured. Push notifications will not work.");
}

// Register services
builder.Services.AddSingleton<IPushNotificationService, FirebaseService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
