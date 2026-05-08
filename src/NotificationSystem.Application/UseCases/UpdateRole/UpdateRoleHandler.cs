using FluentResults;
using MediatR;
using NotificationSystem.Application.Contracts.Roles;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.UpdateRole;

public class UpdateRoleHandler(IRoleManagementService roleManagementService) : IRequestHandler<UpdateRoleCommand, Result<UpdateRoleResponse>>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result<UpdateRoleResponse>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateRoleRequest
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds
        };

        var result = await _roleManagementService.UpdateAsync(request.Id, updateRequest, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<UpdateRoleResponse>(result.Errors);

        return Result.Ok(new UpdateRoleResponse(result.Value));
    }
}
