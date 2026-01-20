using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Factories;

public class PushProviderFactory : ProviderFactoryBase<IPushNotificationService>, IPushProviderFactory
{
    private readonly ILogger<FirebaseService> _firebaseLogger;

    public PushProviderFactory(
        IProviderConfigurationRepository repo,
        ILogger<FirebaseService> firebaseLogger)
        : base(repo)
    {
        _firebaseLogger = firebaseLogger;
    }

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
        // Firebase usa configuração global inicializada no Program.cs
        // Não precisa deserializar aqui pois FirebaseApp já foi inicializado
        return new FirebaseService();
    }
}
