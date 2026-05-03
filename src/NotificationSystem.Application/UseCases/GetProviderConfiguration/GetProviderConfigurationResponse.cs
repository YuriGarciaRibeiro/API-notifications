using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetProviderConfiguration;

public record ProviderConfigurationDetailsResponse(
    Guid Id,
    ChannelType ChannelType,
    ProviderType Provider,
    bool IsActive,
    bool IsPrimary,
    int Priority,
    Dictionary<string, object?> Configuration,
    Dictionary<string, bool> SecretConfigured
);
