namespace NotificationSystem.Application.UseCases.ReprocessAllDLQMessages;

public record ReprocessAllDLQMessagesResponse(
    string Message,
    string DlqName,
    string OriginalQueue
);
