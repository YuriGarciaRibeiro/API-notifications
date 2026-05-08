using FluentResults;
using MediatR;
using NotificationSystem.Application.Contracts.Roles;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.CreateRole;

public class CreateRoleHandler(IRoleManagementService roleManagementService) : IRequestHandler<CreateRoleCommand, Result<CreateRoleResponse>>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result<CreateRoleResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateRoleRequest
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds
        };

        var result = await _roleManagementService.CreateAsync(createRequest, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<CreateRoleResponse>(result.Errors);

        return Result.Ok(new CreateRoleResponse(result.Value));
    }
}
