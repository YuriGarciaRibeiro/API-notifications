using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.CreateRole;

public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, Result<RoleDetailDto>>
{
    private readonly IRoleManagementService _roleManagementService;

    public CreateRoleHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result<RoleDetailDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateRoleRequest
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds
        };

        return await _roleManagementService.CreateAsync(createRequest, cancellationToken);
    }
}
