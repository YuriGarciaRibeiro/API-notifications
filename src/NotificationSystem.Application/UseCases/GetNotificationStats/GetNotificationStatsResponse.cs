namespace NotificationSystem.Application.UseCases.GetNotificationStats;

public record GetNotificationStatsResponse
{
    public int Total { get; init; }
    public int Sent { get; init; }
    public int Pending { get; init; }
    public int Failed { get; init; }
    public ChannelStats ByChannel { get; init; } = new();
    public TrendStats Trends { get; init; } = new();
}

public record ChannelStats
{
    public int Email { get; init; }
    public int Sms { get; init; }
    public int Push { get; init; }
}

public record TrendStats
{
    public TrendValue Total { get; init; } = new();
    public TrendValue Sent { get; init; } = new();
    public TrendValue Pending { get; init; } = new();
    public TrendValue Failed { get; init; } = new();
}

public record TrendValue
{
    public int Value { get; init; }
    public bool IsPositive { get; init; }
}
