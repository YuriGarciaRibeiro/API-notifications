using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.DeleteUser;

public class DeleteUserHandler(IUserManagementService userManagementService) : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserManagementService _userManagementService = userManagementService;

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        return await _userManagementService.DeleteAsync(request.Id, cancellationToken);
    }
}
