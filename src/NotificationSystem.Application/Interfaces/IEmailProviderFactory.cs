namespace NotificationSystem.Application.Interfaces;

using NotificationSystem.Domain.Entities;

public interface IEmailProviderFactory
{
    public Task<IEmailService> CreateEmailProvider();
    public Task<bool> HasActiveConfigAsync(ChannelType channel);
}