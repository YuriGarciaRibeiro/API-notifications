namespace NotificationSystem.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount);
    Task UpdateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
    Task UpdateNotificationChannelStatusAsync<TChannel>(Guid notificationId, Guid channelId, NotificationStatus status, string? errorMessage = null) where TChannel : NotificationChannel;
    Task<NotificationStats> GetStatsAsync(CancellationToken cancellationToken);
    Task<NotificationStats> GetStatsForPeriodAsync(DateTime start, DateTime end, CancellationToken cancellationToken);
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