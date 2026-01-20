using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Repositories;

public class PermissionRepository(NotificationDbContext context) : IPermissionRepository
{
    private readonly NotificationDbContext _context = context;

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => p.Category == category)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _context.Permissions.AddAsync(permission, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await GetByIdAsync(id, cancellationToken);
        if (permission != null)
        {
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
