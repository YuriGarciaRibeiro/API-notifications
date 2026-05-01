using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.ReprocessAllDLQMessages;

public record ReprocessAllDLQMessagesCommand(
    string QueueName
) : IRequest<Result<ReprocessAllDLQMessagesResponse>>;
