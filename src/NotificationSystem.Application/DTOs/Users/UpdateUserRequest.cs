namespace NotificationSystem.Application.DTOs.Users;

public record UpdateUserRequest
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public bool? IsActive { get; init; }
    public List<Guid>? RoleIds { get; init; }
}
