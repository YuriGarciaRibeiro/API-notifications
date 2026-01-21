namespace NotificationSystem.Application.DTOs.Notifications;

public record EmailChannelDto : ChannelDto
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsBodyHtml { get; init; }
}
