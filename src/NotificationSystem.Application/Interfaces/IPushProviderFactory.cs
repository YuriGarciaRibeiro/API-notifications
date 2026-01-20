namespace NotificationSystem.Application.Interfaces;

public interface IPushProviderFactory
{
    Task<IPushNotificationService> CreatePushProvider();
}