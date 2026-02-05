using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Factories;

public class EmailProviderFactory(
    IProviderConfigurationRepository repo,
    ILogger<EmailProviderFactory> logger) : ProviderFactoryBase<IEmailService>(repo), IEmailProviderFactory
{
    private readonly ILogger<EmailProviderFactory> _logger = logger;

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
        _logger.LogInformation("Decrypted SMTP configuration JSON: {ConfigJson}", config.ConfigurationJson);

        // Deserializa o JSON de configuração para SmtpSettings
        var options = DeserializeConfig<SmtpSettings>(config.ConfigurationJson)
            ?? throw new InvalidOperationException("Invalid SMTP configuration");

        _logger.LogInformation(
            "SMTP settings deserialized - Host: {Host}, Port: {Port}, FromEmail: {FromEmail}, EnableSsl: {EnableSsl}",
            options.Host, options.Port, options.FromEmail, options.EnableSsl);

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
