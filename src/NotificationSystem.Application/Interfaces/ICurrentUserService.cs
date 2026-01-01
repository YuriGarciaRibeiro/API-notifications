namespace NotificationSystem.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
    bool HasRole(string role);
}
