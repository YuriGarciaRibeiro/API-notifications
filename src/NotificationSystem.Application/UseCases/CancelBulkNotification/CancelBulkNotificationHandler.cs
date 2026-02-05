using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CancelBulkNotification;

public class CancelBulkNotificationHandler(
    IBulkNotificationRepository repository,
    ILogger<CancelBulkNotificationHandler> logger)
    : IRequestHandler<CancelBulkNotificationCommand, Result>
{
    private readonly IBulkNotificationRepository _repository = repository;
    private readonly ILogger<CancelBulkNotificationHandler> _logger = logger;

    public async Task<Result> Handle(
        CancelBulkNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetWithItemsAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Fail(new Error("NotFound").WithMetadata("message", "Bulk notification job not found"));

        if (job.Status == BulkJobStatus.Completed ||
            job.Status == BulkJobStatus.Failed ||
            job.Status == BulkJobStatus.Cancelled)
        {
            return Result.Fail(new Error("CannotCancelCompletedJob")
                .WithMetadata("message", "Cannot cancel a job that has already completed, failed, or was cancelled"));
        }

        await _repository.UpdateJobStatusAsync(request.JobId, BulkJobStatus.Cancelled, cancellationToken);

        _logger.LogInformation("Bulk notification job {JobId} cancelled", request.JobId);

        return Result.Ok();
    }
}
