using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;

namespace NotificationSystem.Application.UseCases.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string? Name,
    string? Description,
    List<Guid>? PermissionIds) : IRequest<Result<RoleDetailDto>>;
