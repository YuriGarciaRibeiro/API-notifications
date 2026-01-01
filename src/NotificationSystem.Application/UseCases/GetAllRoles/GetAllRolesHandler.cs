using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllRoles;

public class GetAllRolesHandler : IRequestHandler<GetAllRolesQuery, Result<IEnumerable<RoleDetailDto>>>
{
    private readonly IRoleManagementService _roleManagementService;

    public GetAllRolesHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result<IEnumerable<RoleDetailDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        return await _roleManagementService.GetAllAsync(cancellationToken);
    }
}
