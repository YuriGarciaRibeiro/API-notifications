using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllPermissions;

public class GetAllPermissionsHandler(IRoleManagementService roleManagementService) : IRequestHandler<GetAllPermissionsQuery, Result<GetAllPermissionsResponse>>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result<GetAllPermissionsResponse>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetAllPermissionsAsync(cancellationToken);

        if (result.IsFailed)
            return Result.Fail<GetAllPermissionsResponse>(result.Errors);

        return Result.Ok(new GetAllPermissionsResponse(result.Value));
    }
}
