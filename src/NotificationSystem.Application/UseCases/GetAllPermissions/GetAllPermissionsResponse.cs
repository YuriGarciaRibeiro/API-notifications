using NotificationSystem.Application.Contracts.Common;

namespace NotificationSystem.Application.UseCases.GetAllPermissions;

public record GetAllPermissionsResponse(IEnumerable<PermissionDto> Permissions);
