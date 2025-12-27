using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationSystem.Application.Options;

namespace NotificationSystem.Application.Interfaces;

public class SmtpService : ISmtpService
{
    private readonly SmtpOptions _smtpOptions;

    public SmtpService(IOptions<SmtpOptions> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        var secureSocketOptions = _smtpOptions.EnableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, secureSocketOptions);
        await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}