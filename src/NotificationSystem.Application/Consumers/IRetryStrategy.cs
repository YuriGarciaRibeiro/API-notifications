namespace NotificationSystem.Application.Consumers;

public interface IRetryStrategy
{
    bool ShouldRetry(int attemptNumber, Exception exception);
    TimeSpan GetRetryDelay(int attemptNumber);
}
