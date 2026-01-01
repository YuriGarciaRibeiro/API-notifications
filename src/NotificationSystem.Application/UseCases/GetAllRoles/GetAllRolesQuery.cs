using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;

namespace NotificationSystem.Application.UseCases.GetAllRoles;

public record GetAllRolesQuery : IRequest<Result<IEnumerable<RoleDetailDto>>>;
