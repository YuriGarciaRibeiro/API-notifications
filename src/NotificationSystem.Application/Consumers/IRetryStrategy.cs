namespace NotificationSystem.Application.Consumers;

public interface IRetryStrategy
{
    public bool ShouldRetry(int attemptNumber, Exception exception);
    public TimeSpan GetRetryDelay(int attemptNumber);
}
