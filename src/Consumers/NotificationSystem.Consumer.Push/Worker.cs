using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Options;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Worker.Push;

public class Worker : RabbitMqConsumerBase<PushChannelMessage>
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    protected override string QueueName => "push-notifications";

    public Worker(
        ILogger<Worker> logger,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IServiceProvider serviceProvider)
        : base(logger, rabbitMqOptions, serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessMessageAsync(
        PushChannelMessage message,
        CancellationToken cancellationToken)
    {
        // TODO: Integrate with Push notification provider (FCM, APNS, etc.)
        _logger.LogInformation(
            "Sending push notification to {To}: {Title} - {Body}",
            message.To,
            message.Content.Title,
            message.Content.Body);

        // Simulate push notification sending
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Push notification sent successfully");

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        await repository.UpdateNotificationChannelStatusAsync<PushChannel>(
            message.NotificationId,
            message.ChannelId,
            NotificationStatus.Sent);
    }

    protected override Task<(Guid NotificationId, Guid ChannelId)> GetNotificationIdsAsync(
        PushChannelMessage message)
    {
        return Task.FromResult((message.NotificationId, message.ChannelId));
    }

    protected override Type GetChannelType()
    {
        return typeof(PushChannel);
    }
}
