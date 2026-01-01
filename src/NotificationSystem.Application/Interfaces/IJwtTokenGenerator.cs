using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
}
