namespace NotificationSystem.Application.Interfaces;

public interface IEmailProviderFactory
{
    Task<IEmailService> CreateEmailProvider();
}