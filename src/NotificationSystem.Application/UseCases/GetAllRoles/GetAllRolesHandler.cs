using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllRoles;

public class GetAllRolesHandler(IRoleManagementService roleManagementService) : IRequestHandler<GetAllRolesQuery, Result<GetAllRolesResponse>>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result<GetAllRolesResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetAllAsync(cancellationToken);

        return result.IsFailed ? Result.Fail<GetAllRolesResponse>(result.Errors) : Result.Ok(new GetAllRolesResponse(result.Value));
    }
}
