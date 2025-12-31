using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetNotificationStats;

public record GetNotificationStatsQuery : IRequest<Result<GetNotificationStatsResponse>>;
