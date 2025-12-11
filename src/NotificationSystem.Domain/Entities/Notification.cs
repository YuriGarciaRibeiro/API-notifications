using NotificationSystem.Domain.Events;

namespace NotificationSystem.Domain.Entities;

public class Notification
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<NotificationChannel> Channels { get; set; } = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void PublishToAllChannels()
    {
        _domainEvents.Add(new NotificationCreatedEvent(Id));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}