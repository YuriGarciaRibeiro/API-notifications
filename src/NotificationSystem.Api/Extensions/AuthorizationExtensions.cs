using Microsoft.AspNetCore.Authorization;
using NotificationSystem.Application.Authorization;

namespace NotificationSystem.Api.Extensions;

/// <summary>
/// Extension methods for authorization configuration.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds permission-based authorization policies.
    /// </summary>
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Add a policy for each permission
            foreach (var permission in Permissions.GetAll())
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireClaim("permission", permission));
            }
        });

        return services;
    }
}
