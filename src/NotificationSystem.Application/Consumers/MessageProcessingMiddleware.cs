using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Consumers;

public class MessageProcessingMiddleware<TMessage> where TMessage : class
{
    private readonly ILogger<MessageProcessingMiddleware<TMessage>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRetryStrategy _retryStrategy;

    public MessageProcessingMiddleware(
        ILogger<MessageProcessingMiddleware<TMessage>> logger,
        IServiceProvider serviceProvider,
        IRetryStrategy retryStrategy)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _retryStrategy = retryStrategy;
    }

    public async Task<ProcessingResult> ProcessWithErrorHandlingAsync(
        TMessage message,
        Func<TMessage, CancellationToken, Task> processFunc,
        Func<TMessage, Task<(Guid NotificationId, Guid ChannelId)>> getIdsFunc,
        Type channelType,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        int attemptNumber = 0;

        do
        {
            try
            {
                _logger.LogInformation(
                    "Processing message (Attempt {Attempt})",
                    attemptNumber + 1);

                // Executar o processamento
                await processFunc(message, cancellationToken);

                _logger.LogInformation("Message processed successfully");

                return ProcessingResult.Success();
            }
            catch (Exception ex)
            {
                lastException = ex;

                _logger.LogWarning(
                    ex,
                    "Error processing message on attempt {Attempt}. Error: {ErrorMessage}",
                    attemptNumber + 1,
                    ex.Message);

                // Verificar se deve fazer retry
                if (_retryStrategy.ShouldRetry(attemptNumber, ex))
                {
                    var delay = _retryStrategy.GetRetryDelay(attemptNumber);

                    _logger.LogInformation(
                        "Retrying in {DelaySeconds} seconds...",
                        delay.TotalSeconds);

                    await Task.Delay(delay, cancellationToken);
                    attemptNumber++;
                }
                else
                {
                    break;
                }
            }
        } while (true);

        // Se chegou aqui, todas as tentativas falharam
        _logger.LogError(
            lastException,
            "Message processing failed after all retry attempts");

        // Tentar atualizar status no banco
        await TryUpdateNotificationStatusAsync(
            message,
            getIdsFunc,
            channelType,
            NotificationStatus.Failed);

        return ProcessingResult.Failure(lastException!);
    }

    private async Task TryUpdateNotificationStatusAsync(
        TMessage message,
        Func<TMessage, Task<(Guid NotificationId, Guid ChannelId)>> getIdsFunc,
        Type channelType,
        NotificationStatus status)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var (notificationId, channelId) = await getIdsFunc(message);

            _logger.LogInformation(
                "Attempting to update notification {NotificationId}, channel {ChannelId} to status {Status} for channel type {ChannelType}",
                notificationId,
                channelId,
                status,
                channelType.Name);

            var method = typeof(INotificationRepository)
                .GetMethod(nameof(INotificationRepository.UpdateNotificationChannelStatusAsync))
                ?.MakeGenericMethod(channelType);

            if (method != null)
            {
                var task = method.Invoke(
                    repository,
                    new object?[] { notificationId, channelId, status, null });

                if (task is Task t)
                {
                    await t;
                }

                _logger.LogInformation(
                    "✅ Notification {NotificationId} channel {ChannelId} status successfully updated to {Status}",
                    notificationId,
                    channelId,
                    status);
            }
            else
            {
                _logger.LogError(
                    "Method UpdateNotificationChannelStatusAsync not found via reflection for type {ChannelType}",
                    channelType.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Failed to update notification status to {Status}. Manual intervention may be required.",
                status);
        }
    }
}

public class ProcessingResult
{
    public bool IsSuccess { get; init; }
    public Exception? Exception { get; init; }

    public static ProcessingResult Success() => new() { IsSuccess = true };
    public static ProcessingResult Failure(Exception exception) => new()
    {
        IsSuccess = false,
        Exception = exception
    };
}
