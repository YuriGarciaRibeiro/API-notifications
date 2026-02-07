using MediatR;
using NotificationSystem.Domain.Events;

namespace NotificationSystem.Application.Common;

public class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
