using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.Services;

public class AwsSesEmailService(IOptions<AwsSesSettings> settings) : IEmailService
{
    private readonly AwsSesSettings _settings = settings.Value;

    public async Task SendEmailAsync(string recipient, string subject, string body, bool isHtml = false)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient is required", nameof(recipient));

        if (string.IsNullOrWhiteSpace(_settings.Region))
            throw new InvalidOperationException("AWS SES region is required");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new InvalidOperationException("AWS SES fromEmail is required");

        var request = new SendEmailRequest
        {
            Source = BuildSource(),
            Destination = new Destination
            {
                ToAddresses = [recipient]
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = isHtml ? new Content(body) : null,
                    Text = isHtml ? null : new Content(body)
                }
            }
        };

        using var client = CreateClient();
        var response = await client.SendEmailAsync(request);

        if ((int)response.HttpStatusCode is < 200 or >= 300)
        {
            throw new InvalidOperationException(
                $"Failed to send email with AWS SES. StatusCode: {(int)response.HttpStatusCode}");
        }
    }

    private IAmazonSimpleEmailService CreateClient()
    {
        var region = RegionEndpoint.GetBySystemName(_settings.Region);

        var hasAccessKey = !string.IsNullOrWhiteSpace(_settings.AccessKeyId);
        var hasSecret = !string.IsNullOrWhiteSpace(_settings.SecretAccessKey);

        if (hasAccessKey != hasSecret)
        {
            throw new InvalidOperationException(
                "AWS SES accessKeyId and secretAccessKey must be provided together");
        }

        if (hasAccessKey && hasSecret)
        {
            AWSCredentials credentials = string.IsNullOrWhiteSpace(_settings.SessionToken)
                ? new BasicAWSCredentials(_settings.AccessKeyId, _settings.SecretAccessKey)
                : new SessionAWSCredentials(
                    _settings.AccessKeyId,
                    _settings.SecretAccessKey,
                    _settings.SessionToken);

            return new AmazonSimpleEmailServiceClient(credentials, region);
        }

        return new AmazonSimpleEmailServiceClient(region);
    }

    private string BuildSource()
    {
        return string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromEmail
            : $"{_settings.FromName} <{_settings.FromEmail}>";
    }
}
