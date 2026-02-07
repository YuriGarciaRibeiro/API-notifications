using Hangfire.Dashboard;

namespace NotificationSystem.Api.Infrastructure.Hangfire;

/// <summary>
/// Permite acesso ao dashboard sem autenticação (apenas Development)
/// </summary>
public class HangfireDashboardNoAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
