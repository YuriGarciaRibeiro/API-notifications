using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

public interface IPermissionRepository
{
    public Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Permission>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    public Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
