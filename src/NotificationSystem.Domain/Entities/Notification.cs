using NotificationSystem.Domain.Events;
using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Domain.Entities;

public class Notification : IAuditable
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public NotificationOrigin Origin { get; set; }
    public NotificationType Type { get; set; }
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
    Failed,
    Cancelled,
    Scheduled,
    Recurring
}

public enum NotificationOrigin
{
    User,      // Usuário via UI
    Api,       // Sistema externo via API Key (futuro)
    System,    // Gerado automaticamente pelo sistema (eventos, triggers, jobs)
    Scheduled  // Notificação agendada
}

public enum NotificationType
{
    Unique,
    Bulk,
    Campaign
}