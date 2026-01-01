namespace NotificationSystem.Application.DTOs.Auth;

public record RegisterUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public List<Guid> RoleIds { get; init; } = new();
}
