using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotificationSystem.Application.Services;

public class TwilioSmsService : ISmsService
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IOptions<TwilioSettings> settings, ILogger<TwilioSmsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Inicializa o cliente Twilio com as credenciais
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task SendSmsAsync(string to, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {To}", to);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_settings.FromPhoneNumber),
                to: new PhoneNumber(to)
            );

            _logger.LogInformation(
                "SMS sent successfully. MessageSid: {MessageSid}, Status: {Status}",
                messageResource.Sid,
                messageResource.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {To}", to);
            throw;
        }
    }
}