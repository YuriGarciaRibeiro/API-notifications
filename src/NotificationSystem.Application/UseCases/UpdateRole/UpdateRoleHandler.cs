using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.UpdateRole;

public class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDetailDto>>
{
    private readonly IRoleManagementService _roleManagementService;

    public UpdateRoleHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result<RoleDetailDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateRoleRequest
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds
        };

        return await _roleManagementService.UpdateAsync(request.Id, updateRequest, cancellationToken);
    }
}
