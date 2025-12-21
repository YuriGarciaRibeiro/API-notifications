using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Infrastructure.Persistence;
using NotificationSystem.Infrastructure.Persistence.Repositories;
using NotificationSystem.Infrastructure.Settings;

namespace NotificationSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Settings
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        services.Configure<RabbitMQSettings>(configuration.GetSection(RabbitMQSettings.SectionName));

        // Database
        services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
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

        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Message Publisher (RabbitMQ)
        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();

        return services;
    }
}
