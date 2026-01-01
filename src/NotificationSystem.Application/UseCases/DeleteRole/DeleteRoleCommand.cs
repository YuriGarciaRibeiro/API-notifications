using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest<Result>;
