using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetRoleById;

public class GetRoleByIdHandler(IRoleManagementService roleManagementService) : IRequestHandler<GetRoleByIdQuery, Result<GetRoleByIdResponse>>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result<GetRoleByIdResponse>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetByIdAsync(request.Id, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<GetRoleByIdResponse>(result.Errors);

        return Result.Ok(new GetRoleByIdResponse(result.Value));
    }
}
