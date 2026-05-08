using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetUserById;

public class GetUserByIdHandler(IUserManagementService userManagementService) : IRequestHandler<GetUserByIdQuery, Result<GetUserByIdResponse>>
{
    private readonly IUserManagementService _userManagementService = userManagementService;

    public async Task<Result<GetUserByIdResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetByIdAsync(request.Id, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<GetUserByIdResponse>(result.Errors);

        return Result.Ok(new GetUserByIdResponse(result.Value));
    }
}
