namespace NotificationSystem.Application.Interfaces;
public interface IProviderConfigurationRepository
{
    public Task<ProviderConfiguration?> GetActiveProviderAsync(ChannelType channelType, CancellationToken cancellationToken);
    public Task<List<ProviderConfiguration>> GetAllProvidersAsync(CancellationToken cancellationToken);
    public Task<bool> HasAnyProviderForChannelAsync(ChannelType channelType, CancellationToken cancellationToken);
    public Task CreateAsync(ProviderConfiguration providerConfiguration, CancellationToken cancellationToken);
    public Task UpdateAsync(ProviderConfiguration providerConfiguration, CancellationToken cancellationToken);
    public Task SetAsPrimaryAsync(Guid providerConfigurationId, CancellationToken cancellationToken);
    public Task ToggleActiveStatusAsync(Guid providerConfigurationId, CancellationToken cancellationToken);
    public Task DeleteAsync(Guid providerConfigurationId, CancellationToken cancellationToken);
}