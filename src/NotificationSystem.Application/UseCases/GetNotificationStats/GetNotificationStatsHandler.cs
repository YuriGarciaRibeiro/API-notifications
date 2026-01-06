using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetNotificationStats;

public class GetNotificationStatsHandler(INotificationRepository notificationRepository)
    : IRequestHandler<GetNotificationStatsQuery, Result<GetNotificationStatsResponse>>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<Result<GetNotificationStatsResponse>> Handle(
        GetNotificationStatsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);

        var currentStats = await _notificationRepository.GetStatsAsync(cancellationToken);

        var currentWeekStats = await _notificationRepository.GetStatsForPeriodAsync(weekAgo, now, cancellationToken);

        var previousWeekStats = await _notificationRepository.GetStatsForPeriodAsync(weekAgo.AddDays(-7), weekAgo, cancellationToken);

        var response = new GetNotificationStatsResponse
        {
            Total = currentStats.Total,
            Sent = currentStats.Sent,
            Pending = currentStats.Pending,
            Failed = currentStats.Failed,
            ByChannel = new ChannelStats
            {
                Email = currentStats.EmailCount,
                Sms = currentStats.SmsCount,
                Push = currentStats.PushCount
            },
            Trends = new TrendStats
            {
                Total = CalculateTrend(currentWeekStats.Total, previousWeekStats.Total),
                Sent = CalculateTrend(currentWeekStats.Sent, previousWeekStats.Sent),
                Pending = CalculateTrend(currentWeekStats.Pending, previousWeekStats.Pending),
                Failed = CalculateTrend(currentWeekStats.Failed, previousWeekStats.Failed)
            }
        };

        return Result.Ok(response);
    }

    private static TrendValue CalculateTrend(int current, int previous)
    {
        if (previous == 0)
        {
            return new TrendValue
            {
                Value = current > 0 ? 100 : 0,
                IsPositive = current > 0
            };
        }

        var percentChange = ((current - previous) * 100) / previous;

        return new TrendValue
        {
            Value = Math.Abs(percentChange),
            IsPositive = percentChange > 0
        };
    }
}
