using MediatR;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Common.Behaviors;

public class DomainEventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IMediator _mediator;

    public DomainEventDispatcherBehavior(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        // Check if response contains a Notification with domain events
        if (response is FluentResults.Result result && result.IsSuccess)
        {
            // Try to extract notification from result value
            var notification = TryExtractNotification(result);
            if (notification != null && notification.DomainEvents.Any())
            {
                await DispatchDomainEventsAsync(notification, cancellationToken);
            }
        }

        return response;
    }

    private static Notification? TryExtractNotification(object obj)
    {
        // Use reflection to check if the result contains a Notification
        var type = obj.GetType();

        // Check if it's a Result<Notification>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FluentResults.Result<>))
        {
            var valueProperty = type.GetProperty("Value");
            if (valueProperty != null)
            {
                var value = valueProperty.GetValue(obj);
                if (value is Notification notification)
                {
                    return notification;
                }
            }
        }

        return null;
    }

    private async Task DispatchDomainEventsAsync(Notification notification, CancellationToken cancellationToken)
    {
        var events = notification.DomainEvents.ToList();
        notification.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
