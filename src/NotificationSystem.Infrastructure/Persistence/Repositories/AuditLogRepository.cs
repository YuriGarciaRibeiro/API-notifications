using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for AuditLog entity providing read-only access with advanced filtering and pagination.
/// </summary>
public class AuditLogRepository(NotificationDbContext context) : IAuditLogRepository
{
    private readonly NotificationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets paginated list of all audit logs ordered by timestamp descending.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _context.Set<AuditLog>()
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets total count of all audit logs.
    /// </summary>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a specific audit log by ID.
    /// </summary>
    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Set<AuditLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets paginated audit history for a specific entity.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityName,
        string entityId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _context.Set<AuditLog>()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets count of audit logs for a specific entity.
    /// </summary>
    public async Task<int> GetEntityAuditCountAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(
                a => a.EntityName == entityName && a.EntityId == entityId,
                cancellationToken);
    }

    /// <summary>
    /// Gets paginated audit logs for a specific user.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _context.Set<AuditLog>()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets paginated audit logs with advanced filtering by entity, user, action type, and date range.
    /// All filter parameters are optional.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetFilteredAsync(
        string? entityName,
        string? entityId,
        Guid? userId,
        AuditAction? actionType,
        DateTime? startDate,
        DateTime? endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<AuditLog>().AsQueryable();

        // Apply optional filters
        if (!string.IsNullOrEmpty(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrEmpty(entityId))
            query = query.Where(a => a.EntityId == entityId);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        if (actionType.HasValue)
            query = query.Where(a => a.ActionType == actionType);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets count of audit logs matching the same filter criteria as GetFilteredAsync.
    /// </summary>
    public async Task<int> GetFilteredCountAsync(
        string? entityName,
        string? entityId,
        Guid? userId,
        AuditAction? actionType,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<AuditLog>().AsQueryable();

        // Apply same filters as GetFilteredAsync
        if (!string.IsNullOrEmpty(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrEmpty(entityId))
            query = query.Where(a => a.EntityId == entityId);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        if (actionType.HasValue)
            query = query.Where(a => a.ActionType == actionType);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate);

        return await query
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }
}
