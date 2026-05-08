using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllUsers;

public class GetAllUsersHandler(IUserManagementService userManagementService) : IRequestHandler<GetAllUsersQuery, Result<GetAllUsersResponse>>
{
    private readonly IUserManagementService _userManagementService = userManagementService;

    public async Task<Result<GetAllUsersResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetAllAsync(cancellationToken);

        if (result.IsFailed)
            return Result.Fail<GetAllUsersResponse>(result.Errors);

        return Result.Ok(new GetAllUsersResponse(result.Value));
    }
}
