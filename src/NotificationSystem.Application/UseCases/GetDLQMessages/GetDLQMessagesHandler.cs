using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetDLQMessages;

public class GetDLQMessagesHandler(IDeadLetterQueueService deadLetterQueueService)
    : IRequestHandler<GetDLQMessagesQuery, Result<GetDLQMessagesResponse>>
{
    private readonly IDeadLetterQueueService _deadLetterQueueService = deadLetterQueueService;

    public async Task<Result<GetDLQMessagesResponse>> Handle(
        GetDLQMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var messages = await _deadLetterQueueService.GetDeadLetterMessagesAsync(request.QueueName, request.Limit);
        return Result.Ok(new GetDLQMessagesResponse(messages));
    }
}
