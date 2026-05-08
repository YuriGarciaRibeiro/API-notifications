using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.CreateUser;

public class CreateUserHandler(IUserManagementService userManagementService) : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IUserManagementService _userManagementService = userManagementService;

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.CreateAsync(
            request.Email,
            request.Password,
            request.FullName,
            request.RoleIds,
            cancellationToken);

        if (result.IsFailed)
            return Result.Fail<CreateUserResponse>(result.Errors);

        return Result.Ok(new CreateUserResponse(result.Value));
    }
}
