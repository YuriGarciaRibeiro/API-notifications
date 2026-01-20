using System.Text.Json;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Factories;

public abstract class ProviderFactoryBase<TService>(IProviderConfigurationRepository repo) where TService : class
{
    protected readonly IProviderConfigurationRepository _repo = repo;

    protected async Task<ProviderConfiguration> GetActiveConfigAsync(ChannelType channel)
    {
        var config = await _repo.GetActiveProviderAsync(channel, CancellationToken.None);
        if (config == null)
            throw new InvalidOperationException($"No active provider for {channel}");
        return config;
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
