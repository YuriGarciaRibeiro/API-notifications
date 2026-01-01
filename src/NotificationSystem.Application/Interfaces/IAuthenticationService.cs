using FluentResults;
using NotificationSystem.Application.DTOs.Auth;

namespace NotificationSystem.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}
