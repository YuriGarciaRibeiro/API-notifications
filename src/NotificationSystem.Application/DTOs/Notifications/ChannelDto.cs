using System.Text.Json.Serialization;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.DTOs.Notifications;

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
