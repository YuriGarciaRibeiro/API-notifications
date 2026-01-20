using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public IEnumerable<string> Permissions =>
        _httpContextAccessor.HttpContext?.User?.FindAll("permission").Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }

    public bool HasRole(string role)
    {
        return Roles.Contains(role);
    }
}
