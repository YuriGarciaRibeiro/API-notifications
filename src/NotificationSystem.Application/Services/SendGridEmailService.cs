using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Settings;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationSystem.Application.Services;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridSettings _settings;
    private readonly ISendGridClient _client;

    public SendGridEmailService(IOptions<SendGridSettings> settings)
    {
        _settings = settings.Value;
        _client = new SendGridClient(_settings.ApiKey);
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        
        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var toAddress = new EmailAddress(to);

        var msg = MailHelper.CreateSingleEmail(
            from,
            toAddress,
            subject,
            isHtml ? null : body,  // plainTextContent
            isHtml ? body : null   // htmlContent
        );

        var response = await _client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            throw new Exception($"Failed to send email. Status: {response.StatusCode}");
        }
        
    }
}
