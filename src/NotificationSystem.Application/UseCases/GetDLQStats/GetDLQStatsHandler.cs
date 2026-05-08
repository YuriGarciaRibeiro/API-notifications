using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetDLQStats;

public class GetDLQStatsHandler(IDeadLetterQueueService deadLetterQueueService)
    : IRequestHandler<GetDLQStatsQuery, Result<GetDLQStatsResponse>>
{
    private readonly IDeadLetterQueueService _deadLetterQueueService = deadLetterQueueService;

    public async Task<Result<GetDLQStatsResponse>> Handle(
        GetDLQStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await _deadLetterQueueService.GetAllDeadLetterQueueStatsAsync();
        return Result.Ok(new GetDLQStatsResponse(stats));
    }
}
