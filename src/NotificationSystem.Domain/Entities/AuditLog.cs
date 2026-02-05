namespace NotificationSystem.Domain.Entities;

/// <summary>
/// Audit log entity tracking all entity changes in the system.
/// Automatically captures WHO did WHAT, WHEN, and HOW entities changed.
/// </summary>
public class AuditLog
{
    /// <summary>Unique identifier for the audit log entry</summary>
    public Guid Id { get; set; }

    /// <summary>User ID who performed the action (null for system operations)</summary>
    public Guid? UserId { get; set; }

    /// <summary>User email who performed the action (for readability)</summary>
    public string? UserEmail { get; set; }

    /// <summary>Entity type name (e.g., "User", "Notification", "Role")</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Entity ID as string (supports various ID types)</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Type of action performed (Created, Updated, Deleted)</summary>
    public AuditAction ActionType { get; set; }

    /// <summary>JSON representation of old values (null for Created actions)</summary>
    public string? OldValues { get; set; }

    /// <summary>JSON representation of new values (null for Deleted actions)</summary>
    public string? NewValues { get; set; }

    /// <summary>Array of property names that were changed (null for Created/Deleted)</summary>
    public List<string>? ChangedProperties { get; set; }

    /// <summary>When the action occurred (UTC)</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Client IP address (may be forwarded if behind reverse proxy)</summary>
    public string? IpAddress { get; set; }

    /// <summary>Client user agent (browser, mobile app, etc.)</summary>
    public string? UserAgent { get; set; }

    /// <summary>API endpoint that was called</summary>
    public string? RequestPath { get; set; }
}

/// <summary>
/// Enumeration of action types tracked in audit logs.
/// </summary>
public enum AuditAction
{
    /// <summary>Entity was created</summary>
    Created = 1,

    /// <summary>Entity was updated</summary>
    Updated = 2,

    /// <summary>Entity was deleted</summary>
    Deleted = 3
}
