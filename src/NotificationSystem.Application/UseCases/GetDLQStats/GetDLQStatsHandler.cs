using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.DeadLetter;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetDLQStats;

public class GetDLQStatsHandler(IDeadLetterQueueService deadLetterQueueService)
    : IRequestHandler<GetDLQStatsQuery, Result<IEnumerable<DeadLetterQueueStatsDto>>>
{
    private readonly IDeadLetterQueueService _deadLetterQueueService = deadLetterQueueService;

    public async Task<Result<IEnumerable<DeadLetterQueueStatsDto>>> Handle(
        GetDLQStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await _deadLetterQueueService.GetAllDeadLetterQueueStatsAsync();
        return Result.Ok(stats);
    }
}
