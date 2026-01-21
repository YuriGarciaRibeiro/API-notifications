namespace NotificationSystem.Application.DTOs.DeadLetter;

public record DeadLetterQueueStatsDto
{
    public required string QueueName { get; init; }
    public uint MessageCount { get; init; }
    public uint ConsumerCount { get; init; }
}
