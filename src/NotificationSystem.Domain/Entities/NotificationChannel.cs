using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Domain.Entities;

public abstract class NotificationChannel : IAuditable
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public ChannelType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}

public class EmailChannel : NotificationChannel
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsBodyHtml { get; set; } = false;

    public EmailChannel()
    {
        Type = ChannelType.Email;
    }
}

public class SmsChannel : NotificationChannel
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SenderId { get; set; }

    public SmsChannel()
    {
        Type = ChannelType.Sms;
    }
}

public class PushChannel : NotificationChannel
{
    public string To { get; set; } = string.Empty;
    public NotificationContent Content { get; set; } = new();
    public Dictionary<string, string> Data { get; set; } = new();
    public AndroidConfig? Android { get; set; }
    public ApnsConfig? Apns { get; set; }
    public WebpushConfig? Webpush { get; set; }
    public string? Condition { get; set; }
    public int? TimeToLive { get; set; }
    public string? Priority { get; set; }
    public bool? MutableContent { get; set; }
    public bool? ContentAvailable { get; set; }
    public bool IsRead { get; set; } = false;
    public string Platform { get; set; } = string.Empty;

    public PushChannel()
    {
        Type = ChannelType.Push;
    }
}

public enum ChannelType
{
    Email,
    Sms,
    Push
}

public class NotificationContent
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ClickAction { get; set; }
}

public class AndroidConfig
{
    public string? Priority { get; set; }
    public string? Ttl { get; set; }
}

public class ApnsConfig
{
    public Dictionary<string, string>? Headers { get; set; }
}

public class WebpushConfig
{
    public Dictionary<string, string>? Headers { get; set; }
}
