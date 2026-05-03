using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetAllProviders;

public record ProviderConfigurationResponse(
    Guid Id,
    ChannelType ChannelType,
    ProviderType Provider,
    bool IsActive,
    bool IsPrimary,
    int Priority
);
