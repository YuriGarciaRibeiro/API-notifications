using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetBulkNotificationProgress;

public class GetBulkNotificationProgressHandler(IBulkNotificationRepository repository)
    : IRequestHandler<GetBulkNotificationProgressQuery, Result<BulkNotificationProgressResponse>>
{
    private readonly IBulkNotificationRepository _repository = repository;

    public async Task<Result<BulkNotificationProgressResponse>> Handle(
        GetBulkNotificationProgressQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetWithItemsAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Fail(new Error("NotFound").WithMetadata("message", "Bulk notification job not found"));

        var percentComplete = job.TotalCount > 0
            ? (job.ProcessedCount / (double)job.TotalCount * 100)
            : 0;

        var response = new BulkNotificationProgressResponse(
            job.Id,
            job.Name,
            job.Status.ToString(),
            job.TotalCount,
            job.ProcessedCount,
            job.SuccessCount,
            job.FailedCount,
            percentComplete.ToString("F2") + "%",
            job.StartedAt,
            job.CompletedAt,
            job.CreatedAt);

        return Result.Ok(response);
    }
}
