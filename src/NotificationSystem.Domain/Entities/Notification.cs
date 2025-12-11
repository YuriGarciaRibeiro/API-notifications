namespace NotificationSystem.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<NotificationChannel> Channels { get; set; } = new();
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}