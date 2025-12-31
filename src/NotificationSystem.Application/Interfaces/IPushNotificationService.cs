using NotificationSystem.Application.Messages;

namespace NotificationSystem.Application.Interfaces;

public interface IPushNotificationService
{
    Task<string> SendPushNotificationAsync(PushChannelMessage message);
}