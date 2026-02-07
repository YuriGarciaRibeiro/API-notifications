namespace NotificationSystem.Apllication.Interfaces;

public interface INotificationService
{
    public Task PublishNotificationAsync(Guid notificationId);
}
