namespace NotificationSystem.Domain.Entities;

public class SmsNotification : Notification
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SenderId { get; set; }
}