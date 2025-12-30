using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Options;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Worker.Sms;

public class Worker : RabbitMqConsumerBase<SmsChannelMessage>
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    protected override string QueueName => "sms-notifications";

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
        SmsChannelMessage message,
        CancellationToken cancellationToken)
    {
        // TODO: Integrate with SMS provider (Twilio, AWS SNS, etc.)
        _logger.LogInformation(
            "Sending SMS to {To}: {Message}",
            message.To,
            message.Message);

        // Simulate SMS sending
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("SMS sent successfully");

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        await repository.UpdateNotificationChannelStatusAsync<SmsChannel>(
            message.NotificationId,
            message.ChannelId,
            NotificationStatus.Sent);
    }

    protected override Task<(Guid NotificationId, Guid ChannelId)> GetNotificationIdsAsync(
        SmsChannelMessage message)
    {
        return Task.FromResult((message.NotificationId, message.ChannelId));
    }

    protected override Type GetChannelType()
    {
        return typeof(SmsChannel);
    }
}
