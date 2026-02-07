using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NotificationSystem.Application.Common.Behaviors;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;

namespace NotificationSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(DomainEventDispatcherBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // Auth Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();

        return services;
    }
}
