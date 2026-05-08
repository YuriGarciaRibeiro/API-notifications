namespace NotificationSystem.Application.Contracts.Notifications;

public record ChannelRequest(
    string Type,
    Dictionary<string, object> Data
);
