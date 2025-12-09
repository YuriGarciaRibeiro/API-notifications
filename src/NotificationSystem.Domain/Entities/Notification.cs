namespace NotificationSystem.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}

public enum NotificationType
{
    Push,
    Email,
    Sms
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}