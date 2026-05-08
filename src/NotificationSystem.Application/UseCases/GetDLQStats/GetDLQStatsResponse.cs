using NotificationSystem.Application.Contracts.DeadLetter;

namespace NotificationSystem.Application.UseCases.GetDLQStats;

public record GetDLQStatsResponse(IEnumerable<DeadLetterQueueStatsDto> Stats);
