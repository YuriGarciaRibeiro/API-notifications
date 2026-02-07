using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetBulkNotificationJob;

public record GetBulkNotificationJobQuery(Guid JobId) : IRequest<Result<BulkNotificationJobDetailResponse>>;

public record BulkNotificationJobDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int TotalCount,
    int ProcessedCount,
    int SuccessCount,
    int FailureCount,
    double PercentComplete,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime? ScheduledFor,
    string? RecurringCron,
    string? TimeZone,
    string? HangfireJobId,
    bool IsRecurring,
    bool IsScheduled,
    List<string> ErrorMessages
);
