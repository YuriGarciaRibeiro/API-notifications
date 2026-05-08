using FluentResults;
using MediatR;
using NotificationSystem.Application.Contracts.Auth;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.Login;

public class LoginHandler(IAuthenticationService authenticationService) : IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _authenticationService.LoginAsync(loginRequest, request.IpAddress, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<LoginCommandResponse>(result.Errors);

        return Result.Ok(LoginCommandResponse.FromDto(result.Value));
    }
}
