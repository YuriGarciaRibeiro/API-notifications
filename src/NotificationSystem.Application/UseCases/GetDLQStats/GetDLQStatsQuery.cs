using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.DeadLetter;

namespace NotificationSystem.Application.UseCases.GetDLQStats;

public record GetDLQStatsQuery : IRequest<Result<IEnumerable<DeadLetterQueueStatsDto>>>;
