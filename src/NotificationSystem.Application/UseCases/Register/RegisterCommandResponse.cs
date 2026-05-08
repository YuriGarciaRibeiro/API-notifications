using NotificationSystem.Application.Contracts.Auth;

namespace NotificationSystem.Application.UseCases.Register;

public record RegisterCommandResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserInfo User { get; init; } = null!;

    public static RegisterCommandResponse FromDto(LoginResponse response)
    {
        return new RegisterCommandResponse
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = response.ExpiresAt,
            User = response.User
        };
    }
}
