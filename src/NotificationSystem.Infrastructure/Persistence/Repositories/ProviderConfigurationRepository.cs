using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Repositories;

public class ProviderConfigurationRepository(NotificationDbContext context, IEncryptionService encryptionService) : IProviderConfigurationRepository
{
    private readonly NotificationDbContext _context = context;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<ProviderConfiguration?> GetActiveProviderAsync(ChannelType channelType, CancellationToken cancellationToken)
    {
        // Busca o provedor primário E ativo para o canal
        var provider = await _context.ProviderConfigurations
            .FirstOrDefaultAsync(pc => pc.ChannelType == channelType && pc.IsPrimary && pc.IsActive, cancellationToken);
        if (provider != null)
        {
            provider.ConfigurationJson = _encryptionService.Decrypt(provider.ConfigurationJson);
        }

        return provider;
    }

    public async Task<List<ProviderConfiguration>> GetAllProvidersAsync(CancellationToken cancellationToken)
    {
        var providers = await _context.ProviderConfigurations.ToListAsync(cancellationToken);

        foreach (var provider in providers)
        {
            provider.ConfigurationJson = _encryptionService.Decrypt(provider.ConfigurationJson);
        }

        return providers;
    }

    public async Task<bool> HasAnyProviderForChannelAsync(ChannelType channelType, CancellationToken cancellationToken)
    {
        return await _context.ProviderConfigurations
            .AnyAsync(pc => pc.ChannelType == channelType, cancellationToken);
    }

    public async Task CreateAsync(ProviderConfiguration providerConfiguration, CancellationToken cancellationToken)
    {
        providerConfiguration.ConfigurationJson = _encryptionService.Encrypt(providerConfiguration.ConfigurationJson);
        _context.ProviderConfigurations.Add(providerConfiguration);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProviderConfiguration providerConfiguration, CancellationToken cancellationToken)
    {
        providerConfiguration.ConfigurationJson = _encryptionService.Encrypt(providerConfiguration.ConfigurationJson);
        _context.ProviderConfigurations.Update(providerConfiguration);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetAsPrimaryAsync(Guid providerConfigurationId, CancellationToken cancellationToken)
    {
        var providerToSetPrimary = await _context.ProviderConfigurations.FindAsync(providerConfigurationId);
        if (providerToSetPrimary == null)
            throw new InvalidOperationException("Provider configuration not found.");

        // Remove isPrimary de todos os provedores do mesmo canal
        var currentPrimaryProviders = await _context.ProviderConfigurations
            .Where(pc => pc.ChannelType == providerToSetPrimary.ChannelType && pc.IsPrimary)
            .ToListAsync(cancellationToken);
        // Primeiro, remove o isPrimary dos outros provedores
        foreach (var provider in currentPrimaryProviders)
        {
            provider.IsPrimary = false;
        }

        // Salva as mudanças para remover o isPrimary dos outros antes de definir o novo
        await _context.SaveChangesAsync(cancellationToken);

        // Agora define o provedor selecionado como primário e ativo
        providerToSetPrimary.IsPrimary = true;
        providerToSetPrimary.IsActive = true;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task ToggleActiveStatusAsync(Guid providerConfigurationId, CancellationToken cancellationToken)
    {
        return _context.ProviderConfigurations
            .Where(pc => pc.Id == providerConfigurationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(pc => pc.IsActive, pc => !pc.IsActive), cancellationToken);
    }

    public Task DeleteAsync(Guid providerConfigurationId, CancellationToken cancellationToken)
    {
        return _context.ProviderConfigurations
            .Where(pc => pc.Id == providerConfigurationId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}