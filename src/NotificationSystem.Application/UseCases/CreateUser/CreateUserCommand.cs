using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.CreateUser;

public record CreateUserCommand(
    string Email,
    string Password,
    string FullName,
    List<Guid> RoleIds) : IRequest<Result<CreateUserResponse>>;
