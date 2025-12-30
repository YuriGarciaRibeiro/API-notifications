using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Options;
using NotificationSystem.Application.Services;
using NotificationSystem.Application.Settings;
using NotificationSystem.Infrastructure;
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

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// Register Infrastructure (Database, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Register services
builder.Services.AddSingleton<ISmsService, TwilioService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
