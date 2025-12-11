namespace NotificationSystem.Domain.Events;

public record NotificationCreatedEvent(Guid NotificationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
