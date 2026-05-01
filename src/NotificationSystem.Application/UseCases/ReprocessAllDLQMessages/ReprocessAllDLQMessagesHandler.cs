using FluentResults;
using MediatR;
using NotificationSystem.Application.Common.Mappings;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.ReprocessAllDLQMessages;

public class ReprocessAllDLQMessagesHandler(IDeadLetterQueueService deadLetterQueueService)
    : IRequestHandler<ReprocessAllDLQMessagesCommand, Result<ReprocessAllDLQMessagesResponse>>
{
    private readonly IDeadLetterQueueService _deadLetterQueueService = deadLetterQueueService;

    public async Task<Result<ReprocessAllDLQMessagesResponse>> Handle(
        ReprocessAllDLQMessagesCommand request,
        CancellationToken cancellationToken)
    {
        if (!DeadLetterQueueMapping.TryGetOriginalQueue(request.QueueName, out var originalQueue))
        {
            return Result.Fail($"Invalid queue name. Valid queues: {string.Join(", ", DeadLetterQueueMapping.ValidQueues)}");
        }

        await _deadLetterQueueService.ReprocessAllMessagesAsync(request.QueueName, originalQueue);

        var response = new ReprocessAllDLQMessagesResponse(
            "All messages reprocessed successfully",
            request.QueueName,
            originalQueue);

        return Result.Ok(response);
    }
}
