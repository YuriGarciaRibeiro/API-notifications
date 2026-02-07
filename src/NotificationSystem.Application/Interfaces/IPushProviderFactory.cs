namespace NotificationSystem.Application.Interfaces;

using NotificationSystem.Domain.Entities;

public interface IPushProviderFactory
{
    public Task<IPushNotificationService> CreatePushProvider();
    public Task<bool> HasActiveConfigAsync(ChannelType channel);
}