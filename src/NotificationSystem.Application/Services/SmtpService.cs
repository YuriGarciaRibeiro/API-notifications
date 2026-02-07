using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.Services;

public class SmtpService(IOptions<SmtpSettings> smtpOptions) : IEmailService
{
    private readonly SmtpSettings _smtpOptions = smtpOptions.Value;

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart(isHtml ? "html" : "plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        var secureSocketOptions = _smtpOptions.EnableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, secureSocketOptions);

        // Only authenticate if credentials are provided
        if (!string.IsNullOrEmpty(_smtpOptions.Username) && !string.IsNullOrEmpty(_smtpOptions.Password))
        {
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}