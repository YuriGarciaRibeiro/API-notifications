using NotificationSystem.Application.Contracts.Auth;

namespace NotificationSystem.Application.UseCases.Login;

public record LoginCommandResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserInfo User { get; init; } = null!;

    public static LoginCommandResponse FromDto(LoginResponse response)
    {
        return new LoginCommandResponse
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = response.ExpiresAt,
            User = response.User
        };
    }
}
