namespace NotificationSystem.Application.Interfaces;

using NotificationSystem.Domain.Entities;

public interface IPushProviderFactory
{
    Task<IPushNotificationService> CreatePushProvider();
    Task<bool> HasActiveConfigAsync(ChannelType channel);
}