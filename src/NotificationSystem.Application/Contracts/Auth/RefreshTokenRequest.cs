namespace NotificationSystem.Application.Contracts.Auth;

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
