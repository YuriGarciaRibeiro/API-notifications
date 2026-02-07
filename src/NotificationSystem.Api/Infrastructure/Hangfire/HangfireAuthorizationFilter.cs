using Hangfire.Dashboard;
using System.Security.Claims;

namespace NotificationSystem.Api.Infrastructure.Hangfire;

/// <summary>
/// Requer autenticação e permissão específica para acessar o dashboard
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Deve estar autenticado
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            return false;

        // Deve ter a permission "manage.campaigns" ou ser Admin
        var hasPermission = httpContext.User.HasClaim(ClaimTypes.Role, "Admin") ||
                           httpContext.User.HasClaim("permission", "manage.campaigns");

        return hasPermission;
    }
}
