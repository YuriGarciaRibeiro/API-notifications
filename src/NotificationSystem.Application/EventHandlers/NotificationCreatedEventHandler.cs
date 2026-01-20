using MediatR;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Common;
using NotificationSystem.Application.Messages;
using NotificationSystem.Domain.Events;

namespace NotificationSystem.Application.EventHandlers;

public class NotificationCreatedEventHandler(
    INotificationRepository repository,
    IMessagePublisher messagePublisher,
    ILogger<NotificationCreatedEventHandler> logger) : INotificationHandler<DomainEventNotification<NotificationCreatedEvent>>
{
    private readonly INotificationRepository _repository = repository;
    private readonly IMessagePublisher _messagePublisher = messagePublisher;
    private readonly ILogger<NotificationCreatedEventHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<NotificationCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Handling NotificationCreatedEvent for notification {NotificationId}", domainEvent.NotificationId);

        var notificationEntity = await _repository.GetByIdAsync(domainEvent.NotificationId);
        if (notificationEntity == null)
        {
            _logger.LogWarning("Notification {NotificationId} not found", domainEvent.NotificationId);
            return;
        }

        var publishTasks = new List<Task>();

        foreach (var channel in notificationEntity.Channels)
        {
            Task publishTask = channel switch
            {
                EmailChannel emailChannel => PublishEmailChannelAsync(emailChannel, cancellationToken),
                SmsChannel smsChannel => PublishSmsChannelAsync(smsChannel, cancellationToken),
                PushChannel pushChannel => PublishPushChannelAsync(pushChannel, cancellationToken),
                _ => Task.CompletedTask
            };

            publishTasks.Add(publishTask);
        }

        await Task.WhenAll(publishTasks);

        _logger.LogInformation("Published {ChannelCount} channels for notification {NotificationId}",
            notificationEntity.Channels.Count, domainEvent.NotificationId);
    }

    private async Task PublishEmailChannelAsync(EmailChannel channel, CancellationToken cancellationToken)
    {
        var message = new EmailChannelMessage(
            ChannelId: channel.Id,
            NotificationId: channel.NotificationId,
            To: channel.To,
            Subject: channel.Subject,
            Body: channel.Body,
            IsBodyHtml: channel.IsBodyHtml
        );

        await _messagePublisher.PublishAsync("email-notifications", message, cancellationToken);
        _logger.LogInformation("Published email channel {ChannelId} to queue", channel.Id);
    }

    private async Task PublishSmsChannelAsync(SmsChannel channel, CancellationToken cancellationToken)
    {
        var message = new SmsChannelMessage(
            ChannelId: channel.Id,
            NotificationId: channel.NotificationId,
            To: channel.To,
            Message: channel.Message,
            SenderId: channel.SenderId
        );

        await _messagePublisher.PublishAsync("sms-notifications", message, cancellationToken);
        _logger.LogInformation("Published SMS channel {ChannelId} to queue", channel.Id);
    }

    private async Task PublishPushChannelAsync(PushChannel channel, CancellationToken cancellationToken)
    {
        var message = new PushChannelMessage(
            ChannelId: channel.Id,
            NotificationId: channel.NotificationId,
            To: channel.To,
            Content: new PushContentMessage(
                Title: channel.Content.Title,
                Body: channel.Content.Body,
                ClickAction: channel.Content.ClickAction
            ),
            Data: channel.Data,
            Platform: channel.Platform,
            Priority: channel.Priority,
            TimeToLive: channel.TimeToLive,
            Android: MapAndroidConfig(channel.Android),
            Apns: MapApnsConfig(channel.Apns),
            Webpush: MapWebpushConfig(channel.Webpush),
            Condition: channel.Condition,
            MutableContent: channel.MutableContent,
            ContentAvailable: channel.ContentAvailable
        );

        await _messagePublisher.PublishAsync("push-notifications", message, cancellationToken);
        _logger.LogInformation("Published push channel {ChannelId} to queue", channel.Id);
    }

    private static AndroidConfigMessage? MapAndroidConfig(AndroidConfig? config)
    {
        if (config == null) return null;

        return new AndroidConfigMessage(
            Priority: config.Priority,
            Ttl: config.Ttl,
            CollapseKey: null,
            RestrictedPackageName: null,
            Notification: null
        );
    }

    private static ApnsConfigMessage? MapApnsConfig(ApnsConfig? config)
    {
        if (config == null) return null;

        return new ApnsConfigMessage(
            Headers: config.Headers,
            Payload: null
        );
    }

    private static WebpushConfigMessage? MapWebpushConfig(WebpushConfig? config)
    {
        if (config == null) return null;

        return new WebpushConfigMessage(
            Headers: config.Headers,
            Notification: null,
            FcmOptions: null
        );
    }
}
