using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Common;

namespace NotificationSystem.Application.UseCases.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>;
