using FluentResults;
using MediatR;
using NotificationSystem.Application.Contracts.Auth;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.Register;

public class RegisterHandler(IAuthenticationService authenticationService) : IRequestHandler<RegisterCommand, Result<RegisterCommandResponse>>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result<RegisterCommandResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterUserRequest
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName,
            RoleIds = request.RoleIds
        };

        var result = await _authenticationService.RegisterAsync(registerRequest, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<RegisterCommandResponse>(result.Errors);

        return Result.Ok(RegisterCommandResponse.FromDto(result.Value));
    }
}
