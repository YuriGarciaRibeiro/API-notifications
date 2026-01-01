namespace NotificationSystem.Application.DTOs.Roles;

public record CreateRoleRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<Guid> PermissionIds { get; init; } = new();
}
