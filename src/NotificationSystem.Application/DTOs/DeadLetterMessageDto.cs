namespace NotificationSystem.Application.DTOs;

public class DeadLetterMessageDto
{
    public required string QueueName { get; set; }
    public required string MessageBody { get; set; }
    public ulong DeliveryTag { get; set; }
    public DateTime? Timestamp { get; set; }
    public Dictionary<string, object>? Headers { get; set; }
    public string? ErrorReason { get; set; }
    public int RetryCount { get; set; }
}

public class DeadLetterQueueStatsDto
{
    public required string QueueName { get; set; }
    public uint MessageCount { get; set; }
    public uint ConsumerCount { get; set; }
}

public class ReprocessMessageRequest
{
    public required string QueueName { get; set; }
    public ulong DeliveryTag { get; set; }
    public bool ReprocessAll { get; set; }
}
