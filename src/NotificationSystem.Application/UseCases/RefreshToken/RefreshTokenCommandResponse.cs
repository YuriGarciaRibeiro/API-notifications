using NotificationSystem.Application.Contracts.Auth;

namespace NotificationSystem.Application.UseCases.RefreshToken;

public record RefreshTokenCommandResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserInfo User { get; init; } = null!;

    public static RefreshTokenCommandResponse FromDto(LoginResponse response)
    {
        return new RefreshTokenCommandResponse
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = response.ExpiresAt,
            User = response.User
        };
    }
}
