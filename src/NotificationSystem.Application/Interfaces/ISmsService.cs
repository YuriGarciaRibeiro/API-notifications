namespace NotificationSystem.Application.Interfaces;

public interface ISmsService
{
    public Task SendSmsAsync(string phoneNumber, string message);
}