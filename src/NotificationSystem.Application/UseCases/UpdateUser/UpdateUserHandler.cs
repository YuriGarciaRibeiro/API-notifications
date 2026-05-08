using FluentResults;
using MediatR;
using NotificationSystem.Application.Contracts.Users;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.UpdateUser;

public class UpdateUserHandler(IUserManagementService userManagementService) : IRequestHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    private readonly IUserManagementService _userManagementService = userManagementService;

    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateUserRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            IsActive = request.IsActive,
            RoleIds = request.RoleIds
        };

        var result = await _userManagementService.UpdateAsync(request.Id, updateRequest, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<UpdateUserResponse>(result.Errors);

        return Result.Ok(new UpdateUserResponse(result.Value));
    }
}
