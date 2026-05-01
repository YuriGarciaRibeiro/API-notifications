namespace NotificationSystem.Application.UseCases.PurgeDLQ;

public record PurgeDLQResponse(
    string Message,
    string QueueName
);
