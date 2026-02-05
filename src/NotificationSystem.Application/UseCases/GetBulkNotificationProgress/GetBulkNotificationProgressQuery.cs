using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetBulkNotificationProgress;

public record GetBulkNotificationProgressQuery(Guid JobId) : IRequest<Result<BulkNotificationProgressResponse>>;

public record BulkNotificationProgressResponse(
    Guid JobId,
    string Name,
    string Status,
    int TotalCount,
    int ProcessedCount,
    int SuccessCount,
    int FailureCount,
    string PercentComplete,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt
);
