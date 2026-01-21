namespace NotificationSystem.Application.DTOs.Notifications;

public record ChannelRequest(
    string Type,
    Dictionary<string, object> Data
);
