namespace NotificationSystem.Application.Interfaces;

public interface ICurrentUserService
{
    public Guid? UserId { get; }
    public string? Email { get; }
    public IEnumerable<string> Roles { get; }
    public IEnumerable<string> Permissions { get; }
    public bool IsAuthenticated { get; }
    public bool HasPermission(string permission);
    public bool HasRole(string role);
}
