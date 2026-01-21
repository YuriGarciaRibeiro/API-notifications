using NotificationSystem.Application.DTOs.Common;

namespace NotificationSystem.Application.DTOs.Roles;

public record RoleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsSystemRole { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<PermissionDto> Permissions { get; init; } = new();
}
