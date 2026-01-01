using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;

namespace NotificationSystem.Application.UseCases.CreateUser;

public record CreateUserCommand(
    string Email,
    string Password,
    string FullName,
    List<Guid> RoleIds) : IRequest<Result<UserDto>>;
