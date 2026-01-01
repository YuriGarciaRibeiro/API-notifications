using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
    Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
