using FluentResults;
using MediatR;
using NotificationSystem.Application.Common.Mappings;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.ReprocessDLQMessage;

public class ReprocessDLQMessageHandler(IDeadLetterQueueService deadLetterQueueService)
    : IRequestHandler<ReprocessDLQMessageCommand, Result<ReprocessDLQMessageResponse>>
{
    private readonly IDeadLetterQueueService _deadLetterQueueService = deadLetterQueueService;

    public async Task<Result<ReprocessDLQMessageResponse>> Handle(
        ReprocessDLQMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (!DeadLetterQueueMapping.TryGetOriginalQueue(request.QueueName, out var originalQueue))
        {
            return Result.Fail($"Invalid queue name. Valid queues: {string.Join(", ", DeadLetterQueueMapping.ValidQueues)}");
        }

        await _deadLetterQueueService.ReprocessMessageAsync(request.QueueName, originalQueue, request.DeliveryTag);

        var response = new ReprocessDLQMessageResponse(
            "Message reprocessed successfully",
            request.QueueName,
            originalQueue,
            request.DeliveryTag);

        return Result.Ok(response);
    }
}
