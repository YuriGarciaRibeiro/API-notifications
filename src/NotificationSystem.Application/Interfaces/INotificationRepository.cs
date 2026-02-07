namespace NotificationSystem.Application.Interfaces;

public interface INotificationRepository
{
    public Task AddAsync(Notification notification);
    public Task<Notification?> GetByIdAsync(Guid id);
    public Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount);
    public Task UpdateAsync(Notification notification);
    public Task<IEnumerable<Notification>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
    public Task UpdateNotificationChannelStatusAsync<TChannel>(Guid notificationId, Guid channelId, NotificationStatus status, string? errorMessage = null) where TChannel : NotificationChannel;
    public Task<NotificationStats> GetStatsAsync(CancellationToken cancellationToken);
    public Task<NotificationStats> GetStatsForPeriodAsync(DateTime start, DateTime endDate, CancellationToken cancellationToken);
}

public record NotificationStats(
    int Total,
    int Sent,
    int Pending,
    int Failed,
    int EmailCount,
    int SmsCount,
    int PushCount
);