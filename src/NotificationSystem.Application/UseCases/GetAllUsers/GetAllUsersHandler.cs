using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllUsers;

public class GetAllUsersHandler(IUserManagementService userManagementService) : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUserManagementService _userManagementService = userManagementService;

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        return await _userManagementService.GetAllAsync(cancellationToken);
    }
}
