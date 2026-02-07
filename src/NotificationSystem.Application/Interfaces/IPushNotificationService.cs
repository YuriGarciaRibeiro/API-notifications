using NotificationSystem.Application.Messages;

namespace NotificationSystem.Application.Interfaces;

public interface IPushNotificationService
{
    public Task<string> SendPushNotificationAsync(PushChannelMessage message);
}