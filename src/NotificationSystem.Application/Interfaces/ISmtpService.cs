namespace NotificationSystem.Application.Interfaces;

public interface IEmailService
{
    public Task SendEmailAsync(string recipient, string subject, string body, bool isHtml = false);
}