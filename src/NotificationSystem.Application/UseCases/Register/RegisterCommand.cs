using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    List<Guid> RoleIds) : IRequest<Result<RegisterCommandResponse>>;
