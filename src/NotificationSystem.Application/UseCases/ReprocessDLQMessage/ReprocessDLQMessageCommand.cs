using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.ReprocessDLQMessage;

public record ReprocessDLQMessageCommand(
    string QueueName,
    ulong DeliveryTag
) : IRequest<Result<ReprocessDLQMessageResponse>>;
