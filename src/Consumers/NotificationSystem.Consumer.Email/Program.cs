using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Options;
using NotificationSystem.Infrastructure;
using NotificationSystem.Worker.Email;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

// Configure Options
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// Register Infrastructure (Database, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Register services
builder.Services.AddSingleton<ISmtpService, SmtpService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
