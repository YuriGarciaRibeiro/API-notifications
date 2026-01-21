namespace NotificationSystem.Application.DTOs.DeadLetter;

public record DeadLetterMessageDto
{
    public required string QueueName { get; init; }
    public required string MessageBody { get; init; }
    public ulong DeliveryTag { get; init; }
    public DateTime? Timestamp { get; init; }
    public Dictionary<string, object>? Headers { get; init; }
    public string? ErrorReason { get; init; }
    public int RetryCount { get; init; }
}
