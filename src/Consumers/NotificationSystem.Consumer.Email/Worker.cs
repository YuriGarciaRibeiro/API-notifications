using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Worker.Email;

public class Worker : RabbitMqConsumerBase<EmailChannelMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    protected override string QueueName => "email-notifications";

    public Worker(
        ILogger<Worker> logger,
        IOptions<RabbitMqSettings> rabbitMqOptions,
        IServiceProvider serviceProvider,
        MessageProcessingMiddleware<EmailChannelMessage> middleware)
        : base(logger, rabbitMqOptions, serviceProvider, middleware)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(
        EmailChannelMessage message,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        // Verifica se existe um provider de email configurado
        var emailProviderFactory = scope.ServiceProvider.GetRequiredService<IEmailProviderFactory>();
        if (!await emailProviderFactory.HasActiveConfigAsync(ChannelType.Email))
        {
            _logger.LogWarning(
                "Skipping email notification {NotificationId} - No active email provider configured. " +
                "Configure an email provider in the database and set IsActive=true",
                message.NotificationId);
            return;
        }

        // Cria o provedor Email dinamicamente baseado na configuração do banco
        var emailService = await emailProviderFactory.CreateEmailProvider();

        await emailService.SendEmailAsync(
            message.To,
            message.Subject,
            message.Body,
            message.IsBodyHtml);

        _logger.LogInformation("Email sent successfully via {ProviderType}", emailService.GetType().Name);

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
