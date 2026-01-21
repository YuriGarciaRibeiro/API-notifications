namespace NotificationSystem.Application.DTOs.DeadLetter;

public record ReprocessMessageRequest
{
    public required string QueueName { get; init; }
    public ulong DeliveryTag { get; init; }
    public bool ReprocessAll { get; init; }
}
