namespace NotificationSystem.Application.Interfaces;

public interface IMessagePublisher
{
    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
}
