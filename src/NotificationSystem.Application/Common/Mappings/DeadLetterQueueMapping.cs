namespace NotificationSystem.Application.Common.Mappings;

public static class DeadLetterQueueMapping
{
    private static readonly Dictionary<string, string> QueueToOriginalQueue = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sms-notifications-dlq"] = "sms-notifications",
        ["email-notifications-dlq"] = "email-notifications",
        ["push-notifications-dlq"] = "push-notifications"
    };

    public static IReadOnlyCollection<string> ValidQueues => QueueToOriginalQueue.Keys.ToArray();

    public static bool IsValid(string queueName)
    {
        return QueueToOriginalQueue.ContainsKey(queueName);
    }

    public static bool TryGetOriginalQueue(string queueName, out string originalQueueName)
    {
        return QueueToOriginalQueue.TryGetValue(queueName, out originalQueueName!);
    }
}
