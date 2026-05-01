using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.DeadLetter;

namespace NotificationSystem.Application.UseCases.GetDLQMessages;

public record GetDLQMessagesQuery(
    string QueueName,
    int Limit = 100
) : IRequest<Result<IEnumerable<DeadLetterMessageDto>>>;
