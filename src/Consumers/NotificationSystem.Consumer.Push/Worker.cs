using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Worker.Push;

public class Worker : RabbitMqConsumerBase<PushChannelMessage>
{
    private readonly IPushProviderFactory _pushProviderFactory;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    protected override string QueueName => "push-notifications";

    public Worker(
        IPushProviderFactory pushProviderFactory,
        ILogger<Worker> logger,
        IOptions<RabbitMqSettings> rabbitMqOptions,
        IServiceProvider serviceProvider,
        MessageProcessingMiddleware<PushChannelMessage> middleware)
        : base(logger, rabbitMqOptions, serviceProvider, middleware)
    {
        _pushProviderFactory = pushProviderFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessMessageAsync(
        PushChannelMessage message,
        CancellationToken cancellationToken)
    {
        // Cria o provedor Push dinamicamente baseado na configuração do banco
        var pushService = await _pushProviderFactory.CreatePushProvider();

        await pushService.SendPushNotificationAsync(message);

        _logger.LogInformation("Push notification sent successfully via {ProviderType}", pushService.GetType().Name);

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
