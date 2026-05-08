using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string? Name,
    string? Description,
    List<Guid>? PermissionIds) : IRequest<Result<UpdateRoleResponse>>;
