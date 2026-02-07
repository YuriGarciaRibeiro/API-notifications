namespace NotificationSystem.Application.Interfaces;

using NotificationSystem.Domain.Entities;

public interface ISmsProviderFactory
{
    public Task<ISmsService> CreateSmsProvider();
    public Task<bool> HasActiveConfigAsync(ChannelType channel);
}