namespace NotificationSystem.Apllication.Interfaces;

public interface INotificationService
{
    Task PublishNotificationAsync(Guid notificationId);
}
