using NotificationSystem.Application.DTOs.Roles;

namespace NotificationSystem.Application.DTOs.Users;

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public List<RoleDto> Roles { get; init; } = new();
}
