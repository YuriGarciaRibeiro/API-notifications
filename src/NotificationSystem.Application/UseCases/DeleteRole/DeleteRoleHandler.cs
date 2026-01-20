using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.DeleteRole;

public class DeleteRoleHandler(IRoleManagementService roleManagementService) : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        return await _roleManagementService.DeleteAsync(request.Id, cancellationToken);
    }
}
