using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.DTOs.AuditLogs;

/// <summary>
/// Data transfer object for audit log entries.
/// Contains all audit trail information for display in UI.
/// </summary>
public record AuditLogDto
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; init; }

    /// <summary>User ID who performed the action (null for system operations)</summary>
    public Guid? UserId { get; init; }

    /// <summary>User email for readability</summary>
    public string? UserEmail { get; init; }

    /// <summary>Entity type that was changed</summary>
    public string EntityName { get; init; } = string.Empty;

    /// <summary>ID of the specific entity that was changed</summary>
    public string EntityId { get; init; } = string.Empty;

    /// <summary>Action type (Created, Updated, Deleted)</summary>
    public AuditAction ActionType { get; init; }

    /// <summary>JSON snapshot of entity before the change (null for Created)</summary>
    public string? OldValues { get; init; }

    /// <summary>JSON snapshot of entity after the change (null for Deleted)</summary>
    public string? NewValues { get; init; }

    /// <summary>List of property names that were modified (null for Created/Deleted)</summary>
    public List<string>? ChangedProperties { get; init; }

    /// <summary>When the change occurred (UTC)</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Client IP address</summary>
    public string? IpAddress { get; init; }

    /// <summary>Client user agent (browser, mobile app, etc.)</summary>
    public string? UserAgent { get; init; }

    /// <summary>API endpoint that was called</summary>
    public string? RequestPath { get; init; }
}
