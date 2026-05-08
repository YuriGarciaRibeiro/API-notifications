namespace NotificationSystem.Application.Contracts.Users;

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    List<Guid> RoleIds
);
