using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.DTOs.Notifications;

public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public NotificationOrigin Origin { get; init; }
    public NotificationType Type { get; init; }
    public List<ChannelDto> Channels { get; init; } = new();
}
