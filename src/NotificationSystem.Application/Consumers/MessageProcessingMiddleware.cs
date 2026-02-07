using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.Consumers;

public class MessageProcessingMiddleware<TMessage>(
    ILogger<MessageProcessingMiddleware<TMessage>> logger,
    IServiceProvider serviceProvider,
    IRetryStrategy retryStrategy) where TMessage : class
{
    private readonly ILogger<MessageProcessingMiddleware<TMessage>> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IRetryStrategy _retryStrategy = retryStrategy;

    public async Task<ProcessingResult> ProcessWithErrorHandlingAsync(
        TMessage message,
        Func<TMessage, CancellationToken, Task> processFunc,
        Func<TMessage, Task<(Guid NotificationId, Guid ChannelId)>> getIdsFunc,
        Type channelType,
        CancellationToken cancellationToken)
    {
        var attemptNumber = 0;

        Exception? lastException;
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
                if (!_retryStrategy.ShouldRetry(attemptNumber, ex))
                {
                    _logger.LogWarning(
                        "Not retrying message processing after attempt {Attempt}",
                        attemptNumber + 1);
                    break;
                }

                var delay = _retryStrategy.GetRetryDelay(attemptNumber);

                _logger.LogInformation(
                    "Retrying in {DelaySeconds} seconds...",
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
                attemptNumber++;

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
                    [notificationId, channelId, status, null]);

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
