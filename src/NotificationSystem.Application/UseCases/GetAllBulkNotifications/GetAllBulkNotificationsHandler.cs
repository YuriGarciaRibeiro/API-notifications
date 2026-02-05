using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllBulkNotifications;

public class GetAllBulkNotificationsHandler(IBulkNotificationRepository repository)
    : IRequestHandler<GetAllBulkNotificationsQuery, Result<PagedBulkNotificationResponse>>
{
    private readonly IBulkNotificationRepository _repository = repository;

    public async Task<Result<PagedBulkNotificationResponse>> Handle(
        GetAllBulkNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var (jobs, totalCount) = await _repository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Status,
            request.SortBy,
            request.SortOrder,
            cancellationToken);

        var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;
        var hasNextPage = request.Page < totalPages;
        var hasPreviousPage = request.Page > 1;

        var summaries = jobs.Select(j => new BulkNotificationSummary(
            j.Id,
            j.Name,
            j.Description,
            j.Status.ToString(),
            j.TotalCount,
            j.ProcessedCount,
            j.SuccessCount,
            j.FailedCount,
            j.TotalCount > 0 ? (j.ProcessedCount / (double)j.TotalCount * 100) : 0,
            j.CreatedAt,
            j.CompletedAt
        ));

        var response = new PagedBulkNotificationResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages,
            hasNextPage,
            hasPreviousPage
        );

        return Result.Ok(response);
    }
}
