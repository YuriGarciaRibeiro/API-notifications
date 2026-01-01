using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserManagementService _userManagementService;

    public UpdateUserHandler(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateUserRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            IsActive = request.IsActive,
            RoleIds = request.RoleIds
        };

        return await _userManagementService.UpdateAsync(request.Id, updateRequest, cancellationToken);
    }
}
