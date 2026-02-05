namespace NotificationSystem.Application.Interfaces;

using NotificationSystem.Domain.Entities;

public interface ISmsProviderFactory
{
    Task<ISmsService> CreateSmsProvider();
    Task<bool> HasActiveConfigAsync(ChannelType channel);
}