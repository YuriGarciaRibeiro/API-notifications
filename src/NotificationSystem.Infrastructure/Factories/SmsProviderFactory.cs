using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Application.Settings;
using NotificationSystem.Domain.Entities;
using System.Text.Json;

namespace NotificationSystem.Infrastructure.Factories;

public class SmsProviderFactory : ProviderFactoryBase<ISmsService>, ISmsProviderFactory
{
    private readonly ILogger<TwilioService> _twilioLogger;

    public SmsProviderFactory(
        IProviderConfigurationRepository repo,
        ILogger<TwilioService> twilioLogger)
        : base(repo)
    {
        _twilioLogger = twilioLogger;
    }

    public async Task<ISmsService> CreateSmsProvider()
    {
        var config = await GetActiveConfigAsync(ChannelType.Sms);

        return config.Provider switch
        {
            ProviderType.Twilio => CreateTwilio(config),
            _ => throw new NotSupportedException($"SMS Provider '{config.Provider}' is not supported")
        };
    }

    private ISmsService CreateTwilio(ProviderConfiguration config)
    {
        // Deserializa o JSON de configuração para TwilioSettings
        var settings = DeserializeConfig<TwilioSettings>(config.ConfigurationJson)
            ?? throw new InvalidOperationException("Invalid Twilio configuration");

        // Cria e retorna uma instância do TwilioService
        return new TwilioService(
            Options.Create(settings),
            _twilioLogger
        );
    }
}