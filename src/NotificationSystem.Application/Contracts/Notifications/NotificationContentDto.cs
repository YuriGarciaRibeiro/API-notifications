namespace NotificationSystem.Application.Contracts.Notifications;

public record NotificationContentDto
{
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ClickAction { get; init; }
}
