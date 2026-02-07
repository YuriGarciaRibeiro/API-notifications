namespace NotificationSystem.Application.Consumers;

public class ExponentialBackoffRetryStrategy(
    int maxRetries = 3,
    TimeSpan? initialDelay = null,
    TimeSpan? maxDelay = null) : IRetryStrategy
{
    private readonly int _maxRetries = maxRetries;
    private readonly TimeSpan _initialDelay = initialDelay ?? TimeSpan.FromSeconds(2);
    private readonly TimeSpan _maxDelay = maxDelay ?? TimeSpan.FromMinutes(5);

    public bool ShouldRetry(int attemptNumber, Exception exception)
    {
        if (attemptNumber >= _maxRetries) return false;

        // Não tentar novamente para erros permanentes
        return IsTransientError(exception);
    }

    public TimeSpan GetRetryDelay(int attemptNumber)
    {
        var delay = TimeSpan.FromMilliseconds(
            _initialDelay.TotalMilliseconds * Math.Pow(2, attemptNumber));

        return delay > _maxDelay ? _maxDelay : delay;
    }

    private static bool IsTransientError(Exception exception)
    {
        // Erros transitórios (temporários) - vale a pena retry
        return exception is TimeoutException
            or HttpRequestException
            or TaskCanceledException
            or IOException;
    }
}
