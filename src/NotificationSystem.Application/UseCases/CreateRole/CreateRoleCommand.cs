using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;

namespace NotificationSystem.Application.UseCases.CreateRole;

public record CreateRoleCommand(
    string Name,
    string Description,
    List<Guid> PermissionIds) : IRequest<Result<RoleDetailDto>>;
