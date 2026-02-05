using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Entities;
using System.Text.Json;

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
            _ => throw new NotSupportedException($"SMS Provider '{config.Provider}' is not supported")
        };
    }

    public async Task<bool> HasActiveConfigAsync(ChannelType channel)
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