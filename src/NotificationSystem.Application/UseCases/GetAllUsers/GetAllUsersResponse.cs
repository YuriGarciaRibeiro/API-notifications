using NotificationSystem.Application.Contracts.Users;

namespace NotificationSystem.Application.UseCases.GetAllUsers;

public record GetAllUsersResponse(IEnumerable<UserDto> Users);
