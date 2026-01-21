using NotificationSystem.Application.DTOs.DeadLetter;

namespace NotificationSystem.Application.Interfaces;

public interface IDeadLetterQueueService
{
    Task<IEnumerable<DeadLetterQueueStatsDto>> GetAllDeadLetterQueueStatsAsync();
    Task<IEnumerable<DeadLetterMessageDto>> GetDeadLetterMessagesAsync(string queueName, int limit = 100);
    Task ReprocessMessageAsync(string dlqName, string originalQueueName, ulong deliveryTag);
    Task ReprocessAllMessagesAsync(string dlqName, string originalQueueName);
    Task PurgeDeadLetterQueueAsync(string queueName);
}
