using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.PurgeDLQ;

public class PurgeDLQHandler(IDeadLetterQueueService deadLetterQueueService)
    : IRequestHandler<PurgeDLQCommand, Result<PurgeDLQResponse>>
{
    private readonly IDeadLetterQueueService _deadLetterQueueService = deadLetterQueueService;

    public async Task<Result<PurgeDLQResponse>> Handle(
        PurgeDLQCommand request,
        CancellationToken cancellationToken)
    {
        await _deadLetterQueueService.PurgeDeadLetterQueueAsync(request.QueueName);

        var response = new PurgeDLQResponse(
            "Queue purged successfully",
            request.QueueName);

        return Result.Ok(response);
    }
}
