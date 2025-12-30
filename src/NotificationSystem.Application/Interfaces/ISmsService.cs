namespace NotificationSystem.Application.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(string to, string message);
}