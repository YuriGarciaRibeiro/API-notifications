using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserManagementService _userManagementService;

    public ChangePasswordHandler(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        return await _userManagementService.ChangePasswordAsync(request.UserId, changePasswordRequest, cancellationToken);
    }
}
