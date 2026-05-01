using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.PurgeDLQ;

public record PurgeDLQCommand(
    string QueueName
) : IRequest<Result<PurgeDLQResponse>>;
