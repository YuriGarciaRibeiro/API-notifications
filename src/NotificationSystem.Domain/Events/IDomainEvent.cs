namespace NotificationSystem.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
