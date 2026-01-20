using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetRoleById;

public class GetRoleByIdHandler(IRoleManagementService roleManagementService) : IRequestHandler<GetRoleByIdQuery, Result<RoleDetailDto>>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result<RoleDetailDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _roleManagementService.GetByIdAsync(request.Id, cancellationToken);
    }
}
