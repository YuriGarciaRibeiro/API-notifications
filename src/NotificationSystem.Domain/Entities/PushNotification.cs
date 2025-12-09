namespace NotificationSystem.Domain.Entities;

public class PushNotification : Notification
{
    public string To { get; set; } = string.Empty;
    public NotificationContent Content { get; set; } = new NotificationContent();
    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    public AndroidConfig? Android { get; set; }
    public ApnsConfig? Apns { get; set; }
    public WebpushConfig? Webpush { get; set; }
    public string? Condition { get; set; }
    public int? TimeToLive { get; set; }
    public string? Priority { get; set; }
    public bool? MutableContent { get; set; }
    public bool? ContentAvailable { get; set; }
    public bool IsRead { get; set; } = false;

    public PushNotification()
    {
        Type = NotificationType.Push;
    }
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