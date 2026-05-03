namespace NotificationSystem.Api.Hubs;

public sealed record BulkProgressUpdate(
    Guid JobId,
    string Status,
    double Percent,
    int Total,
    int Processed,
    int Success,
    int Failure,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime? UpdatedAt,
    DateTime CheckedAt
);
