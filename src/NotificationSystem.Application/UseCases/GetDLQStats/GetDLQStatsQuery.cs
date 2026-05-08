using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetDLQStats;

public record GetDLQStatsQuery : IRequest<Result<GetDLQStatsResponse>>;
