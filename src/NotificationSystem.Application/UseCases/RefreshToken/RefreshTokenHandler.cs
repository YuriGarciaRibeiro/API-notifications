using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.RefreshToken;

public class RefreshTokenHandler(IAuthenticationService authenticationService) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenCommandResponse>>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result<RefreshTokenCommandResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken, request.IpAddress, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<RefreshTokenCommandResponse>(result.Errors);

        return Result.Ok(RefreshTokenCommandResponse.FromDto(result.Value));
    }
}
