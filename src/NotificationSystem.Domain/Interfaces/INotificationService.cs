namespace NotificationSystem.Domain.Interfaces;

public interface INotificationService
{
    Task PublishNotificationAsync(Guid notificationId);
}