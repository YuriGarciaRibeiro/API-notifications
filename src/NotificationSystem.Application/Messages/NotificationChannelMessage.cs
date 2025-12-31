namespace NotificationSystem.Application.Messages;

public record EmailChannelMessage(
    Guid ChannelId,
    Guid NotificationId,
    string To,
    string Subject,
    string Body,
    bool IsBodyHtml
);

public record SmsChannelMessage(
    Guid ChannelId,
    Guid NotificationId,
    string To,
    string Message,
    string? SenderId
);

public record PushChannelMessage(
    Guid ChannelId,
    Guid NotificationId,
    string To,
    PushContentMessage Content,
    Dictionary<string, string> Data,
    string Platform,
    string? Priority,
    int? TimeToLive,
    AndroidConfigMessage? Android,
    ApnsConfigMessage? Apns,
    WebpushConfigMessage? Webpush,
    string? Condition,
    bool? MutableContent,
    bool? ContentAvailable
);

public record PushContentMessage(
    string Title,
    string Body,
    string? ClickAction
);

public record AndroidConfigMessage(
    string? Priority,
    string? Ttl,
    string? CollapseKey,
    string? RestrictedPackageName,
    AndroidNotificationMessage? Notification
);

public record AndroidNotificationMessage(
    string? Title,
    string? Body,
    string? Icon,
    string? Color,
    string? Sound,
    string? Tag,
    string? ClickAction,
    string? BodyLocKey,
    string[]? BodyLocArgs,
    string? TitleLocKey,
    string[]? TitleLocArgs,
    string? ChannelId,
    int? NotificationPriority,
    int? NotificationCount,
    string? Ticker,
    bool? Sticky,
    string? EventTime,
    bool? LocalOnly,
    string? Visibility,
    string? ImageUrl
);

public record ApnsConfigMessage(
    Dictionary<string, string>? Headers,
    ApnsPayloadMessage? Payload
);

public record ApnsPayloadMessage(
    ApsMessage? Aps,
    Dictionary<string, object>? CustomData
);

public record ApsMessage(
    AlertMessage? Alert,
    int? Badge,
    string? Sound,
    bool? ContentAvailable,
    bool? MutableContent,
    string? Category,
    string? ThreadId
);

public record AlertMessage(
    string? Title,
    string? Body,
    string? LaunchImage,
    string? TitleLocKey,
    string[]? TitleLocArgs,
    string? LocKey,
    string[]? LocArgs
);

public record WebpushConfigMessage(
    Dictionary<string, string>? Headers,
    WebpushNotificationMessage? Notification,
    WebpushFcmOptionsMessage? FcmOptions
);

public record WebpushNotificationMessage(
    string? Title,
    string? Body,
    string? Icon,
    string? Badge,
    string? Image,
    string? Tag,
    string? Data,
    string? Dir,
    string? Lang,
    bool? Renotify,
    bool? RequireInteraction,
    bool? Silent,
    long? Timestamp
);

public record WebpushFcmOptionsMessage(
    string? Link
);
