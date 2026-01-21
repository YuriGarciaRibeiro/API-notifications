namespace NotificationSystem.Application.DTOs.Notifications;

public record SmsChannelDto : ChannelDto
{
    public string To { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? SenderId { get; init; }
}
