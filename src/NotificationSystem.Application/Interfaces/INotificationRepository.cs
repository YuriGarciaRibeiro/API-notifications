namespace NotificationSystem.Apllication.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount);
    Task UpdateAsync(Notification notification);
}