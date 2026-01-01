using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.RevokeToken;

public class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IAuthenticationService _authenticationService;

    public RevokeTokenHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.RevokeTokenAsync(request.RefreshToken, request.IpAddress, cancellationToken);
    }
}
