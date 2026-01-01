using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Auth;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public LoginHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        };

        return await _authenticationService.LoginAsync(loginRequest, request.IpAddress, cancellationToken);
    }
}
