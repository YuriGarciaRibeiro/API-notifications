using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetUserById;

public class GetUserByIdHandler(IUserManagementService userManagementService) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserManagementService _userManagementService = userManagementService;

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _userManagementService.GetByIdAsync(request.Id, cancellationToken);
    }
}
