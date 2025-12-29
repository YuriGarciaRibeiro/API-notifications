using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Options;
using NotificationSystem.Worker.Email;

var builder = Host.CreateApplicationBuilder(args);

// Configure Options
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// Register services
builder.Services.AddSingleton<ISmtpService, SmtpService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
