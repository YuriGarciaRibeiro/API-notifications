using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

public interface IJwtTokenGenerator
{
    public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    public string GenerateRefreshToken();
}
