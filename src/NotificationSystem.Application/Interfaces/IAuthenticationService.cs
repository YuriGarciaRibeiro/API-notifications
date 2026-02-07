using FluentResults;
using NotificationSystem.Application.DTOs.Auth;

namespace NotificationSystem.Application.Interfaces;

public interface IAuthenticationService
{
    public Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    public Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    public Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    public Task<Result<LoginResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}
