using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetProviderMetadata;

public record ProviderMetadataResponse(
    string Version,
    DateTime GeneratedAt,
    List<ProviderChannelMetadataResponse> Channels,
    List<ProviderDefinitionMetadataResponse> Providers
);

public record ProviderChannelMetadataResponse(
    ChannelType Value,
    string Label,
    List<ProviderType> Providers
);

public record ProviderDefinitionMetadataResponse(
    ProviderType Value,
    string Key,
    string Label,
    ChannelType ChannelType,
    bool SupportsUpload,
    string DocsUrl,
    List<ProviderFieldMetadataResponse> Fields
);

public record ProviderFieldMetadataResponse(
    string Name,
    string Label,
    string Type,
    bool Required,
    bool Encrypted,
    string? Placeholder,
    string? HelpText,
    string? Pattern = null,
    int? Min = null,
    int? Max = null,
    int? MinLength = null
);
