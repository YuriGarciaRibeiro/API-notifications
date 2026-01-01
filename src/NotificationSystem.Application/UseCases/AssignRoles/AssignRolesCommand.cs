using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.AssignRoles;

public record AssignRolesCommand(Guid UserId, List<Guid> RoleIds) : IRequest<Result>;
