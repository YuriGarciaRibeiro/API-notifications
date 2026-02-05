using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Infrastructure.Persistence;
using NotificationSystem.Infrastructure.Persistence.Interceptors;
using NotificationSystem.Infrastructure.Persistence.Repositories;
using NotificationSystem.Infrastructure.Services;

namespace NotificationSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Settings
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Audit Log Interceptor
        services.AddScoped<AuditLogInterceptor>();

        // Database
        services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
        {
            var databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

            if (string.IsNullOrEmpty(databaseSettings.ConnectionString))
            {
                throw new InvalidOperationException("Database connection string not found.");
            }

            var auditInterceptor = serviceProvider.GetService<AuditLogInterceptor>();

            options.UseNpgsql(
                databaseSettings.ConnectionString,
                b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)
            );

            if (auditInterceptor != null)
            {
                options.AddInterceptors(auditInterceptor);
            }
        });

        // Data Protection for encryption
        services.AddDataProtection()
            .SetApplicationName("NotificationSystem");

        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IBulkNotificationRepository, BulkNotificationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IProviderConfigurationRepository, ProviderConfigurationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Encryption Service
        services.AddScoped<IEncryptionService, EncryptionService>();

        // Auth Services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Message Publisher (RabbitMQ)
        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();

        return services;
    }

    /// <summary>
    /// Adiciona os Provider Factories (usado apenas pelos Consumers)
    /// </summary>
    public static IServiceCollection AddProviderFactories(this IServiceCollection services)
    {
        services.AddScoped<ISmsProviderFactory, Factories.SmsProviderFactory>();
        services.AddScoped<IEmailProviderFactory, Factories.EmailProviderFactory>();
        services.AddScoped<IPushProviderFactory, Factories.PushProviderFactory>();

        return services;
    }
}
