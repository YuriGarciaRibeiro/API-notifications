using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

public interface IRoleRepository
{
    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    public Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<IEnumerable<Role>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default);
    public Task AddAsync(Role role, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    public Task<bool> HasUsersAsync(Guid roleId, CancellationToken cancellationToken = default);
    public Task AssignPermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
    public Task RemovePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
}
