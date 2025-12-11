using System.Text.Json.Serialization;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetAllNotifications;

public record GetAllNotificationsResponse(
    IEnumerable<NotificationDto> Notifications,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<ChannelDto> Channels { get; init; } = new();
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EmailChannelDto), "Email")]
[JsonDerivedType(typeof(SmsChannelDto), "Sms")]
[JsonDerivedType(typeof(PushChannelDto), "Push")]
public abstract record ChannelDto
{
    public Guid Id { get; init; }
    public NotificationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? SentAt { get; init; }
}

public record EmailChannelDto : ChannelDto
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsBodyHtml { get; init; }
}

public record SmsChannelDto : ChannelDto
{
    public string To { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? SenderId { get; init; }
}

public record PushChannelDto : ChannelDto
{
    public string To { get; init; } = string.Empty;
    public NotificationContentDto Content { get; init; } = new();
    public Dictionary<string, string> Data { get; init; } = new();
    public string? Priority { get; init; }
    public int? TimeToLive { get; init; }
    public bool IsRead { get; init; }
}

public record NotificationContentDto
{
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ClickAction { get; init; }
}
