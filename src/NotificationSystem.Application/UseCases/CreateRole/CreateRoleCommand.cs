using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.CreateRole;

public record CreateRoleCommand(
    string Name,
    string Description,
    List<Guid> PermissionIds) : IRequest<Result<CreateRoleResponse>>;
