using NotificationSystem.Application.DTOs.DeadLetter;

namespace NotificationSystem.Application.Interfaces;

public interface IDeadLetterQueueService
{
    public Task<IEnumerable<DeadLetterQueueStatsDto>> GetAllDeadLetterQueueStatsAsync();
    public Task<IEnumerable<DeadLetterMessageDto>> GetDeadLetterMessagesAsync(string queueName, int limit = 100);
    public Task ReprocessMessageAsync(string dlqName, string originalQueueName, ulong deliveryTag);
    public Task ReprocessAllMessagesAsync(string dlqName, string originalQueueName);
    public Task PurgeDeadLetterQueueAsync(string queueName);
}
