namespace NotificationSystem.Application.Interfaces;

public interface ISmtpService
{
    Task SendEmailAsync(string to, string subject, string body);
}