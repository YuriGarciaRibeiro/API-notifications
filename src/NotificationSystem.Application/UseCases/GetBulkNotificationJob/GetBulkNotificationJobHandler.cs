using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetBulkNotificationJob;

public class GetBulkNotificationJobHandler(IBulkNotificationRepository repository)
    : IRequestHandler<GetBulkNotificationJobQuery, Result<BulkNotificationJobDetailResponse>>
{
    private readonly IBulkNotificationRepository _repository = repository;

    public async Task<Result<BulkNotificationJobDetailResponse>> Handle(
        GetBulkNotificationJobQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetWithItemsAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Fail(new Error("NotFound").WithMetadata("message", "Bulk notification job not found"));

        var percentComplete = job.TotalCount > 0
            ? (job.ProcessedCount / (double)job.TotalCount * 100)
            : 0;

        var response = new BulkNotificationJobDetailResponse(
            job.Id,
            job.Name,
            job.Description,
            job.Status.ToString(),
            job.TotalCount,
            job.ProcessedCount,
            job.SuccessCount,
            job.FailedCount,
            percentComplete,
            job.StartedAt,
            job.CompletedAt,
            job.CreatedAt,
            job.ScheduledFor,
            job.RecurringCron,
            job.TimeZone,
            job.HangfireJobId,
            job.IsRecurring,
            job.IsScheduled,
            job.ErrorMessages);

        return Result.Ok(response);
    }
}
