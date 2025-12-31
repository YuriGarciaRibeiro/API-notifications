namespace NotificationSystem.Application.UseCases.GetNotificationStats;

public record GetNotificationStatsResponse
{
    public int Total { get; init; }
    public int Sent { get; init; }
    public int Pending { get; init; }
    public int Failed { get; init; }
    public ChannelStats ByChannel { get; init; } = new();
}

public record ChannelStats
{
    public int Email { get; init; }
    public int Sms { get; init; }
    public int Push { get; init; }
}
