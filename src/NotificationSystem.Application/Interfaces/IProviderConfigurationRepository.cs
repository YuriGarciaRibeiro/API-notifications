public interface IProviderConfigurationRepository
{
    Task<ProviderConfiguration?> GetActiveProviderAsync(ChannelType channelType);
    Task<List<ProviderConfiguration>> GetAllProvidersAsync();
    Task CreateAsync(ProviderConfiguration providerConfiguration);
    Task UpdateAsync(ProviderConfiguration providerConfiguration);
    Task SetAsPrimaryAsync(Guid providerConfigurationId);
}