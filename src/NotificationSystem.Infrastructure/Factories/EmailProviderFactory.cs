using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Options;
using NotificationSystem.Application.Services;
using NotificationSystem.Application.Settings;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Factories;

public class EmailProviderFactory : ProviderFactoryBase<IEmailService>, IEmailProviderFactory
{
    private readonly ILogger<SmtpService> _smtpLogger;

    public EmailProviderFactory(
        IProviderConfigurationRepository repo,
        ILogger<SmtpService> smtpLogger)
        : base(repo)
    {
        _smtpLogger = smtpLogger;
    }

    public async Task<IEmailService> CreateEmailProvider()
    {
        var config = await GetActiveConfigAsync(ChannelType.Email);

        return config.Provider switch
        {
            ProviderType.Smtp => CreateSmtp(config),
            ProviderType.SendGrid => CreateSendGrid(config),
            _ => throw new NotSupportedException($"Email Provider '{config.Provider}' is not supported")
        };
    }

    private IEmailService CreateSmtp(ProviderConfiguration config)
    {
        // Deserializa o JSON de configuração para SmtpOptions
        var options = DeserializeConfig<SmtpOptions>(config.ConfigurationJson)
            ?? throw new InvalidOperationException("Invalid SMTP configuration");

        // Cria e retorna uma instância do SmtpService
        return new SmtpService(Options.Create(options));
    }

    private IEmailService CreateSendGrid(ProviderConfiguration config)
    {
        // Deserializa o JSON de configuração para SendGridSettings
        var settings = DeserializeConfig<SendGridSettings>(config.ConfigurationJson)
            ?? throw new InvalidOperationException("Invalid SendGrid configuration");

        // Cria e retorna uma instância do SendGridEmailService
        return new SendGridEmailService(
            Options.Create(settings)
        );
    }
}
