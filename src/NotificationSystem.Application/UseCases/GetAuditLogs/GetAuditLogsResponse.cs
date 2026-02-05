using NotificationSystem.Application.DTOs.AuditLogs;

namespace NotificationSystem.Application.UseCases.GetAuditLogs;

/// <summary>
/// Response containing paginated audit logs with metadata.
/// </summary>
/// <param name="AuditLogs">List of audit log entries</param>
/// <param name="TotalCount">Total number of audit logs matching the filter criteria</param>
/// <param name="PageNumber">Current page number (1-based)</param>
/// <param name="PageSize">Number of records per page</param>
public record GetAuditLogsResponse(
    IEnumerable<AuditLogDto> AuditLogs,
    int TotalCount,
    int PageNumber,
    int PageSize
);
