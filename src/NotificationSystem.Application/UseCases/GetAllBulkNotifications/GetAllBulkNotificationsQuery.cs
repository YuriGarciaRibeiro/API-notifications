using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetAllBulkNotifications;

public record GetAllBulkNotificationsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string SortBy = "createdAt",
    string SortOrder = "desc"
) : IRequest<Result<PagedBulkNotificationResponse>>;

public record BulkNotificationSummary(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int TotalCount,
    int ProcessedCount,
    int SuccessCount,
    int FailureCount,
    double PercentComplete,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record PagedBulkNotificationResponse(
    IEnumerable<BulkNotificationSummary> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);
