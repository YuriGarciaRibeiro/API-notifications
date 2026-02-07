using System.Text.Json;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Factories;

public abstract class ProviderFactoryBase<TService>(IProviderConfigurationRepository repo) where TService : class
{
    protected readonly IProviderConfigurationRepository Repo = repo;

    protected async Task<ProviderConfiguration> GetActiveConfigAsync(ChannelType channel)
    {
        var config = await Repo.GetActiveProviderAsync(channel, CancellationToken.None);
        return config ?? throw new InvalidOperationException($"No active provider for {channel}");
    }

    protected async Task<bool> HasActiveConfigAsync(ChannelType channel)
    {
        var config = await Repo.GetActiveProviderAsync(channel, CancellationToken.None);
        return config != null;
    }

    protected T? DeserializeConfig<T>(string json) where T : class
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<T>(json, options);
    }
}
