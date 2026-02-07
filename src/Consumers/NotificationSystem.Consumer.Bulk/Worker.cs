using Microsoft.Extensions.Options;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Worker.Bulk;

/// <summary>
/// Consumer para processar jobs de notificações em massa.
/// Herda de RabbitMqConsumerBase para reutilizar infraestrutura de retry e DLQ.
/// </summary>
public class Worker : RabbitMqConsumerBase<BulkNotificationJobMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    protected override string QueueName => "bulk-notifications";

    public Worker(
        ILogger<Worker> logger,
        IOptions<RabbitMqSettings> rabbitMqOptions,
        IServiceProvider serviceProvider,
        MessageProcessingMiddleware<BulkNotificationJobMessage> middleware)
        : base(logger, rabbitMqOptions, serviceProvider, middleware)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Processamento principal do bulk job.
    /// 1. Busca job e items do banco de dados
    /// 2. Processa cada item: cria notificação e publica para fila específica do canal
    /// 3. Atualiza progresso e status do job
    /// 4. Marca como concluído
    /// </summary>
    protected override async Task ProcessMessageAsync(
        BulkNotificationJobMessage message,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var bulkRepository = scope.ServiceProvider.GetRequiredService<IBulkNotificationRepository>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        _logger.LogInformation("Starting bulk job processing: {JobId}", message.JobId);

        // 1. Buscar job completo do banco de dados (com items)
        var job = await bulkRepository.GetWithItemsAsync(message.JobId, cancellationToken);

        if (job is null)
        {
            _logger.LogError("Bulk job {JobId} not found. Job may have been deleted.", message.JobId);
            throw new InvalidOperationException($"Bulk job {message.JobId} not found");
        }

        // Verificar se job foi cancelado
        if (job.Status == BulkJobStatus.Cancelled)
        {
            _logger.LogInformation("Bulk job {JobId} is cancelled. Skipping processing.", message.JobId);
            return;
        }

        // 2. Marcar job como Processing
        job.Status = BulkJobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;

        await bulkRepository.UpdateJobAsync(job, cancellationToken);

        _logger.LogInformation(
            "Bulk job {JobId} status changed to Processing. Total items: {TotalCount}",
            job.Id,
            job.TotalCount);

        int successCount = 0;
        int failureCount = 0;
        int processedCount = 0;

        // 3. Processar cada item
        foreach (var item in job.Items)
        {
            // Skip se já foi processado
            if (item.Status != NotificationStatus.Pending)
            {
                _logger.LogDebug(
                    "Item {ItemId} already processed with status {Status}. Skipping.",
                    item.Id,
                    item.Status);
                continue;
            }

            try
            {
                // Criar notificação individual
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = job.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    Origin = NotificationOrigin.System,
                    Type = NotificationType.Bulk,
                    Channels = new()
                };

                // Criar channel específico baseado no tipo de canal
                NotificationChannel channel = item.Channel switch
                {
                    ChannelType.Email => new EmailChannel
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        To = item.Recipient,
                        Subject = "Notification",
                        Body = "Check your notification",
                        IsBodyHtml = false
                    },

                    ChannelType.Sms => new SmsChannel
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        To = item.Recipient,
                        Message = "You have a new notification"
                    },

                    ChannelType.Push => new PushChannel
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        To = item.Recipient,
                        Content = new NotificationContent
                        {
                            Title = "Notification",
                            Body = "You have a new notification"
                        },
                        Data = item.Variables ?? new(),
                        Platform = "fcm"
                    },

                    _ => throw new InvalidOperationException($"Unsupported channel type: {item.Channel}")
                };

                notification.Channels.Add(channel);

                // Salvar notificação no banco de dados
                await notificationRepository.AddAsync(notification);

                // Publicar mensagem para o consumer específico do canal
                if (channel is EmailChannel emailChannel)
                {
                    var emailMessage = new EmailChannelMessage(
                        emailChannel.Id,
                        notification.Id,
                        emailChannel.To,
                        emailChannel.Subject,
                        emailChannel.Body,
                        emailChannel.IsBodyHtml);

                    await messagePublisher.PublishAsync("email-notifications", emailMessage, cancellationToken);
                }
                else if (channel is SmsChannel smsChannel)
                {
                    var smsMessage = new SmsChannelMessage(
                        smsChannel.Id,
                        notification.Id,
                        smsChannel.To,
                        smsChannel.Message,
                        smsChannel.SenderId);

                    await messagePublisher.PublishAsync("sms-notifications", smsMessage, cancellationToken);
                }
                else if (channel is PushChannel pushChannel)
                {
                    var pushMessage = new PushChannelMessage(
                        pushChannel.Id,
                        notification.Id,
                        pushChannel.To,
                        new PushContentMessage(
                            pushChannel.Content.Title,
                            pushChannel.Content.Body,
                            pushChannel.Content.ClickAction),
                        pushChannel.Data,
                        pushChannel.Platform,
                        pushChannel.Priority,
                        pushChannel.TimeToLive,
                        null,
                        null,
                        null,
                        pushChannel.Condition,
                        pushChannel.MutableContent,
                        pushChannel.ContentAvailable);

                    await messagePublisher.PublishAsync("push-notifications", pushMessage, cancellationToken);
                }

                // Atualizar item como enviado
                await bulkRepository.UpdateItemStatusAsync(
                    item.Id,
                    item.Status == NotificationStatus.Scheduled ? NotificationStatus.Scheduled : NotificationStatus.Sent,
                    notificationId: notification.Id,
                    cancellationToken: cancellationToken);

                successCount++;

                _logger.LogDebug(
                    "Bulk item {ItemId} processed successfully. Notification {NotificationId} created.",
                    item.Id,
                    notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing bulk item {ItemId} for job {JobId}",
                    item.Id,
                    job.Id);

                // Atualizar item como falhado
                await bulkRepository.UpdateItemStatusAsync(
                    item.Id,
                    NotificationStatus.Failed,
                    ErrorMessage: ex.Message,
                    cancellationToken: cancellationToken);

                failureCount++;

                // Adicionar erro ao job
                await bulkRepository.AddErrorMessageAsync(
                    job.Id,
                    $"Item {item.Recipient}: {ex.Message}",
                    cancellationToken);
            }

            // Atualizar progresso a cada item processado
            await bulkRepository.IncrementProcessedCountAssync(job.Id, job.Status, cancellationToken);
            processedCount++;

            // Log de progresso a cada 100 items
            if (processedCount % 100 == 0)
            {
                double percentComplete = processedCount / (double)job.TotalCount * 100;
                _logger.LogInformation(
                    "Bulk job {JobId} progress: {Processed}/{Total} ({Percent:F2}%)",
                    job.Id,
                    processedCount,
                    job.TotalCount,
                    percentComplete);
            }
        }

        // 4. Finalizar job
        job.Status = BulkJobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.SuccessCount = successCount;
        job.FailedCount = failureCount;
        job.UpdatedAt = DateTime.UtcNow;

        // Persistir mudanças no banco de dados
        await bulkRepository.UpdateJobAsync(job, cancellationToken);

        _logger.LogInformation(
            "Bulk job {JobId} completed: {Success} success, {Failure} failures, Duration: {Duration}s",
            job.Id,
            successCount,
            failureCount,
            (DateTime.UtcNow - job.StartedAt!.Value).TotalSeconds);
    }

    /// <summary>
    /// Requerido pela interface RabbitMqConsumerBase.
    /// Não é usado em bulk processing (não temos notificationId na mensagem).
    /// </summary>
    protected override Task<(Guid NotificationId, Guid ChannelId)> GetNotificationIdsAsync(
        BulkNotificationJobMessage message)
    {
        // Retorna valores dummy - não é usado pois middleware não atualiza status
        return Task.FromResult((Guid.Empty, Guid.Empty));
    }

    /// <summary>
    /// Requerido pela interface RabbitMqConsumerBase.
    /// Retorna EmailChannel para conformidade, mas não é relevante.
    /// </summary>
    protected override Type GetChannelType()
    {
        return typeof(EmailChannel);
    }
}
