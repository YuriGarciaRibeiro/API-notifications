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
    string? Priority,
    int? TimeToLive
);

public record PushContentMessage(
    string Title,
    string Body,
    string? ClickAction
);
