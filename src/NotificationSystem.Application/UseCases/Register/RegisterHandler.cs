using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Auth;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<LoginResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public RegisterHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<LoginResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterUserRequest
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName,
            RoleIds = request.RoleIds
        };

        return await _authenticationService.RegisterAsync(registerRequest, cancellationToken);
    }
}
