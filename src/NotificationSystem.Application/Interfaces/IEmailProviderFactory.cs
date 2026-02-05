namespace NotificationSystem.Application.Interfaces;

using NotificationSystem.Domain.Entities;

public interface IEmailProviderFactory
{
    Task<IEmailService> CreateEmailProvider();
    Task<bool> HasActiveConfigAsync(ChannelType channel);
}