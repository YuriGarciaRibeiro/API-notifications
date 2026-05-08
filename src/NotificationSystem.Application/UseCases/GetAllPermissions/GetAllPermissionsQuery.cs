using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<Result<GetAllPermissionsResponse>>;
