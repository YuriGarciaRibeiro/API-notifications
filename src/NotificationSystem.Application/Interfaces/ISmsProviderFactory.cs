namespace NotificationSystem.Application.Interfaces;

public interface ISmsProviderFactory
{
    Task<ISmsService> CreateSmsProvider();
}