using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Common;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllPermissions;

public class GetAllPermissionsHandler(IRoleManagementService roleManagementService) : IRequestHandler<GetAllPermissionsQuery, Result<IEnumerable<PermissionDto>>>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result<IEnumerable<PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _roleManagementService.GetAllPermissionsAsync(cancellationToken);
    }
}
