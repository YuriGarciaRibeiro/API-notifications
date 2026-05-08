using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetDLQMessages;

public record GetDLQMessagesQuery(
    string QueueName,
    int Limit = 100
) : IRequest<Result<GetDLQMessagesResponse>>;
