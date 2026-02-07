namespace NotificationSystem.Domain.Events;

public interface IDomainEvent
{
    public DateTime OccurredOn { get; }
}
