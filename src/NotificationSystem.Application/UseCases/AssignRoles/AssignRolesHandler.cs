using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.AssignRoles;

public class AssignRolesHandler : IRequestHandler<AssignRolesCommand, Result>
{
    private readonly IUserManagementService _userManagementService;

    public AssignRolesHandler(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<Result> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        return await _userManagementService.AssignRolesAsync(request.UserId, request.RoleIds, cancellationToken);
    }
}
