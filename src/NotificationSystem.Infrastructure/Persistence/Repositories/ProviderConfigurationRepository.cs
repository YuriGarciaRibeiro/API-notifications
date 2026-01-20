using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Repositories;

public class ProviderConfigurationRepository : IProviderConfigurationRepository
{
    private readonly NotificationDbContext _context;
    private readonly IEncryptionService _encryptionService;

    public ProviderConfigurationRepository(NotificationDbContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

    public async Task<ProviderConfiguration?> GetActiveProviderAsync(ChannelType channelType)
    {
        var provider = await _context.ProviderConfigurations
            .FirstOrDefaultAsync(pc => pc.ChannelType == channelType && pc.IsActive);

        if (provider != null)
        {
            provider.ConfigurationJson = _encryptionService.Decrypt(provider.ConfigurationJson);
        }

        return provider;
    }

    public async Task<List<ProviderConfiguration>> GetAllProvidersAsync()
    {
        var providers = await _context.ProviderConfigurations.ToListAsync();

        foreach (var provider in providers)
        {
            provider.ConfigurationJson = _encryptionService.Decrypt(provider.ConfigurationJson);
        }

        return providers;
    }

    public async Task CreateAsync(ProviderConfiguration providerConfiguration)
    {
        providerConfiguration.ConfigurationJson = _encryptionService.Encrypt(providerConfiguration.ConfigurationJson);
        _context.ProviderConfigurations.Add(providerConfiguration);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProviderConfiguration providerConfiguration)
    {
        providerConfiguration.ConfigurationJson = _encryptionService.Encrypt(providerConfiguration.ConfigurationJson);
        _context.ProviderConfigurations.Update(providerConfiguration);
        await _context.SaveChangesAsync();
    }

    public async Task SetAsPrimaryAsync(Guid providerConfigurationId)
    {
        var providerToSetPrimary = await _context.ProviderConfigurations.FindAsync(providerConfigurationId);
        if (providerToSetPrimary == null)
            throw new InvalidOperationException("Provider configuration not found.");

        var currentPrimaryProviders = _context.ProviderConfigurations
            .Where(pc => pc.ChannelType == providerToSetPrimary.ChannelType && pc.IsActive);

        foreach (var provider in currentPrimaryProviders)
        {
            provider.IsActive = false;
        }

        providerToSetPrimary.IsActive = true;

        await _context.SaveChangesAsync();
    }
}