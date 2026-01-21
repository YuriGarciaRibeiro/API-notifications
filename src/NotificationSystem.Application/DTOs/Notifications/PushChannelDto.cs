namespace NotificationSystem.Application.DTOs.Notifications;

public record PushChannelDto : ChannelDto
{
    public string To { get; init; } = string.Empty;
    public NotificationContentDto Content { get; init; } = new();
    public Dictionary<string, string> Data { get; init; } = new();
    public string? Priority { get; init; }
    public int? TimeToLive { get; init; }
    public bool IsRead { get; init; }
}
