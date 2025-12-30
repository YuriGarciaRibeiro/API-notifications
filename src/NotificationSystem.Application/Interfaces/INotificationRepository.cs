namespace NotificationSystem.Apllication.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount);
    Task UpdateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task UpdateNotificationChannelStatusAsync<TChannel>(Guid notificationId, Guid channelId, NotificationStatus status, string? errorMessage = null) where TChannel : NotificationChannel;
}