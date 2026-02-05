using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Interfaces;

/// <summary>
/// Repository interface for reading audit logs with advanced filtering and pagination.
/// Provides read-only access to audit trail data for compliance and troubleshooting.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets paginated list of all audit logs ordered by timestamp descending.
    /// </summary>
    /// <param name="pageNumber">1-based page number</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of audit logs</returns>
    Task<IEnumerable<AuditLog>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets total count of all audit logs.
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a specific audit log by ID.
    /// </summary>
    /// <returns>AuditLog if found, null otherwise</returns>
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets paginated audit history for a specific entity (all changes to one record).
    /// </summary>
    /// <param name="entityName">Type of entity (e.g., "User", "Notification")</param>
    /// <param name="entityId">ID of the specific entity</param>
    /// <param name="pageNumber">1-based page number</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated history of changes to the entity</returns>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityName,
        string entityId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets count of audit log entries for a specific entity.
    /// </summary>
    Task<int> GetEntityAuditCountAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets paginated audit logs for actions performed by a specific user.
    /// </summary>
    /// <param name="userId">ID of the user who performed the actions</param>
    /// <param name="pageNumber">1-based page number</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of audit logs filtered by user</returns>
    Task<IEnumerable<AuditLog>> GetByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets paginated audit logs with advanced filtering.
    /// All filter parameters are optional and combined with AND logic.
    /// </summary>
    /// <param name="entityName">Optional: filter by entity type</param>
    /// <param name="entityId">Optional: filter by entity ID</param>
    /// <param name="userId">Optional: filter by user who performed action</param>
    /// <param name="actionType">Optional: filter by action type (Created, Updated, Deleted)</param>
    /// <param name="startDate">Optional: filter by date range start (UTC)</param>
    /// <param name="endDate">Optional: filter by date range end (UTC)</param>
    /// <param name="pageNumber">1-based page number</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of filtered audit logs</returns>
    Task<IEnumerable<AuditLog>> GetFilteredAsync(
        string? entityName,
        string? entityId,
        Guid? userId,
        AuditAction? actionType,
        DateTime? startDate,
        DateTime? endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets count of audit logs matching the filter criteria.
    /// Uses same filter logic as GetFilteredAsync.
    /// </summary>
    Task<int> GetFilteredCountAsync(
        string? entityName,
        string? entityId,
        Guid? userId,
        AuditAction? actionType,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken);
}
