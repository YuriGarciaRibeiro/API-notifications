using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);
    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<IEnumerable<User>> GetAllWithRolesAsync(CancellationToken cancellationToken = default);
    public Task AddAsync(User user, CancellationToken cancellationToken = default);
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    public Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
