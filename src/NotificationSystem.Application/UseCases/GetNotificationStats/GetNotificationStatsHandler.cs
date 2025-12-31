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
        var stats = await _notificationRepository.GetStatsAsync(cancellationToken);

        var response = new GetNotificationStatsResponse
        {
            Total = stats.Total,
            Sent = stats.Sent,
            Pending = stats.Pending,
            Failed = stats.Failed,
            ByChannel = new ChannelStats
            {
                Email = stats.EmailCount,
                Sms = stats.SmsCount,
                Push = stats.PushCount
            }
        };

        return Result.Ok(response);
    }
}
