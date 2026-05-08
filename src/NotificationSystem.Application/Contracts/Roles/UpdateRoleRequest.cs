namespace NotificationSystem.Application.Contracts.Roles;

public record UpdateRoleRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<Guid>? PermissionIds { get; init; }
}
