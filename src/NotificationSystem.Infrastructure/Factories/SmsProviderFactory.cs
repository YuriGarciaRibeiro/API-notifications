using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Exceptions;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Factories;

public class SmsProviderFactory(
    IProviderConfigurationRepository repo,
    ILogger<TwilioSmsService> twilioLogger) : ProviderFactoryBase<ISmsService>(repo), ISmsProviderFactory
{
    private readonly ILogger<TwilioSmsService> _twilioLogger = twilioLogger;

    public async Task<ISmsService> CreateSmsProvider()
    {
        var config = await GetActiveConfigAsync(ChannelType.Sms);

        return config.Provider switch
        {
            ProviderType.Twilio => CreateTwilio(config),
            ProviderType.Smtp => throw new InvalidProviderTypeException(
                ProviderType.Smtp,
                ChannelType.Sms,
                "SMTP is an Email provider and cannot be used with SMS factory. Use EmailProviderFactory instead."),
            ProviderType.SendGrid => throw new InvalidProviderTypeException(
                ProviderType.SendGrid,
                ChannelType.Sms,
                "SendGrid is an Email provider and cannot be used with SMS factory. Use EmailProviderFactory instead."),
            ProviderType.Firebase => throw new InvalidProviderTypeException(
                ProviderType.Firebase,
                ChannelType.Sms,
                "Firebase is a Push notification provider and cannot be used with SMS factory. Use PushProviderFactory instead."),
            _ => throw new NotSupportedException($"SMS Provider '{config.Provider}' is not supported")
        };
    }

    public new async Task<bool> HasActiveConfigAsync(ChannelType channel)
    {
        return await base.HasActiveConfigAsync(channel);
    }

    private ISmsService CreateTwilio(ProviderConfiguration config)
    {
        // Deserializa o JSON de configuração para TwilioSettings
        var settings = DeserializeConfig<TwilioSettings>(config.ConfigurationJson)
            ?? throw new InvalidOperationException("Invalid Twilio configuration");

        // Cria e retorna uma instância do TwilioSsmService
        return new TwilioSmsService(
            Options.Create(settings),
            _twilioLogger
        );
    }
}