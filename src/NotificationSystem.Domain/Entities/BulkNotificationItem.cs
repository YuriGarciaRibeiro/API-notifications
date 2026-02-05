using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Domain.Entities;

public class BulkNotificationItem : IAuditable
{
    // Identifiers
    public Guid Id { get; set; }
    public Guid BulkJobId { get; set; }

    // Recipient Data
    public string Recipient { get; set; } = string.Empty;                  // email, phone, device_token
    public ChannelType Channel { get; set; }

    // Template Variables {{Name}}, {{Code}}
    public Dictionary<string, string> Variables { get; set; } = [];

    // Status
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }

    // FK para notificação criada
    public Guid? NotificationId { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual BulkNotificationJob BulkJob { get; set; } = null!;
}