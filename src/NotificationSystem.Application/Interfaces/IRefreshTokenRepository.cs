using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

public interface IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    public Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    public Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
