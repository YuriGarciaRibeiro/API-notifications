using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Options;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Worker.Email;

public class Worker : RabbitMqConsumerBase<EmailChannelMessage>
{
    private readonly ISmtpService _smtpService;
    private readonly IServiceProvider _serviceProvider;

    protected override string QueueName => "email-notifications";

    public Worker(
        ILogger<Worker> logger,
        ISmtpService smtpService,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IServiceProvider serviceProvider,
        MessageProcessingMiddleware<EmailChannelMessage> middleware)
        : base(logger, rabbitMqOptions, serviceProvider, middleware)
    {
        _smtpService = smtpService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessMessageAsync(
        EmailChannelMessage message,
        CancellationToken cancellationToken)
    {
        await _smtpService.SendEmailAsync(
            message.To,
            message.Subject,
            message.Body,
            message.IsBodyHtml);

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        await repository.UpdateNotificationChannelStatusAsync<EmailChannel>(
            message.NotificationId,
            message.ChannelId,
            NotificationStatus.Sent);
    }

    protected override Task<(Guid NotificationId, Guid ChannelId)> GetNotificationIdsAsync(
        EmailChannelMessage message)
    {
        return Task.FromResult((message.NotificationId, message.ChannelId));
    }

    protected override Type GetChannelType()
    {
        return typeof(EmailChannel);
    }
}
