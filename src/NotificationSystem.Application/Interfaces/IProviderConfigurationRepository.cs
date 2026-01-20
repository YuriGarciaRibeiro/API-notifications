namespace NotificationSystem.Application.Interfaces;
public interface IProviderConfigurationRepository
{
    Task<ProviderConfiguration?> GetActiveProviderAsync(ChannelType channelType, CancellationToken cancellationToken);
    Task<List<ProviderConfiguration>> GetAllProvidersAsync(CancellationToken cancellationToken);
    Task<bool> HasAnyProviderForChannelAsync(ChannelType channelType, CancellationToken cancellationToken);
    Task CreateAsync(ProviderConfiguration providerConfiguration, CancellationToken cancellationToken);
    Task UpdateAsync(ProviderConfiguration providerConfiguration, CancellationToken cancellationToken);
    Task SetAsPrimaryAsync(Guid providerConfigurationId, CancellationToken cancellationToken);
    Task ToggleActiveStatusAsync(Guid providerConfigurationId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid providerConfigurationId, CancellationToken cancellationToken);
}