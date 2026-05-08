using NotificationSystem.Application.Contracts.Common;

namespace NotificationSystem.Application.Contracts.Roles;

public record RoleDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsSystemRole { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<PermissionDto> Permissions { get; init; } = new();
}
