namespace NotificationSystem.Application.UseCases.ReprocessDLQMessage;

public record ReprocessDLQMessageResponse(
    string Message,
    string DlqName,
    string OriginalQueue,
    ulong DeliveryTag
);
