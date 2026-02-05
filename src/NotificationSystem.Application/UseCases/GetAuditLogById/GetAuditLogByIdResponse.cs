using NotificationSystem.Application.DTOs.AuditLogs;

namespace NotificationSystem.Application.UseCases.GetAuditLogById;

/// <summary>
/// Response containing details of a single audit log entry.
/// </summary>
/// <param name="AuditLog">The audit log entry</param>
public record GetAuditLogByIdResponse(AuditLogDto AuditLog);
