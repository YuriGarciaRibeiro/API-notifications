using NotificationSystem.Application.DTOs.AuditLogs;

namespace NotificationSystem.Application.UseCases.GetEntityAuditHistory;

/// <summary>
/// Response containing paginated audit history for a specific entity.
/// Shows all changes (create, updates, delete) made to one entity record.
/// </summary>
/// <param name="AuditLogs">Audit log entries for the entity</param>
/// <param name="TotalCount">Total number of audit entries for this entity</param>
/// <param name="PageNumber">Current page number (1-based)</param>
/// <param name="PageSize">Number of records per page</param>
public record GetEntityAuditHistoryResponse(
    IEnumerable<AuditLogDto> AuditLogs,
    int TotalCount,
    int PageNumber,
    int PageSize
);
