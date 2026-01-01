using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;

namespace NotificationSystem.Application.UseCases.GetRoleById;

public record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleDetailDto>>;
