using Microsoft.Extensions.Options;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Options;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Worker.Sms;

public class Worker : RabbitMqConsumerBase<SmsChannelMessage>
{
    private readonly ISmsProviderFactory _smsProviderFactory;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    protected override string QueueName => "sms-notifications";

    public Worker(
        ISmsProviderFactory smsProviderFactory,
        ILogger<Worker> logger,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IServiceProvider serviceProvider,
        MessageProcessingMiddleware<SmsChannelMessage> middleware)
        : base(logger, rabbitMqOptions, serviceProvider, middleware)
    {
        _smsProviderFactory = smsProviderFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessMessageAsync(
        SmsChannelMessage message,
        CancellationToken cancellationToken)
    {
        // Cria o provedor SMS dinamicamente baseado na configuração do banco
        var smsService = await _smsProviderFactory.CreateSmsProvider();

        await smsService.SendSmsAsync(message.To, message.Message);

        _logger.LogInformation("SMS sent successfully via {ProviderType}", smsService.GetType().Name);

        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        await notificationRepository.UpdateNotificationChannelStatusAsync<SmsChannel>(
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
