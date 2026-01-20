using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Application.Settings;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Factories;

public class PushProviderFactory(
    IProviderConfigurationRepository repo,
    ILogger<PushProviderFactory> logger,
    IEncryptionService encryptionService) : ProviderFactoryBase<IPushNotificationService>(repo), IPushProviderFactory
{
    private readonly ILogger<PushProviderFactory> _logger = logger;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<IPushNotificationService> CreatePushProvider()
    {
        var config = await GetActiveConfigAsync(ChannelType.Push);

        return config.Provider switch
        {
            ProviderType.Firebase => CreateFirebase(config),
            _ => throw new NotSupportedException($"Push Provider '{config.Provider}' is not supported")
        };
    }

    private IPushNotificationService CreateFirebase(ProviderConfiguration config)
    {
        try
        {
            // Descriptografa a configuração
            var decryptedJson = _encryptionService.Decrypt(config.ConfigurationJson);
            var settings = JsonSerializer.Deserialize<FirebaseSettings>(decryptedJson);

            if (settings == null)
                throw new InvalidOperationException("Failed to deserialize Firebase settings");

            _logger.LogInformation("Creating Firebase service with configuration from database");

            // Prioriza credenciais em JSON (mais seguro)
            if (!string.IsNullOrEmpty(settings.CredentialsJson))
            {
                _logger.LogInformation("Using Firebase credentials from JSON content");
                return new FirebaseService(settings.CredentialsJson);
            }

            // Fallback para arquivo de credenciais
            if (!string.IsNullOrEmpty(settings.CredentialsPath))
            {
                _logger.LogInformation("Using Firebase credentials from file: {Path}", settings.CredentialsPath);
                return new FirebaseService(settings.CredentialsPath, settings.ProjectId);
            }

            throw new InvalidOperationException(
                "Firebase configuration must contain either 'CredentialsJson' or 'CredentialsPath'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Firebase service from database configuration");
            throw;
        }
    }
}
