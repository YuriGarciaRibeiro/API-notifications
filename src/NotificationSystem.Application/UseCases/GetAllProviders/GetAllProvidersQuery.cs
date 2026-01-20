using FluentResults;
using MediatR;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetAllProviders;

public record GetAllProvidersQuery(
    ChannelType? ChannelType = null
) : IRequest<Result<List<ProviderConfigurationResponse>>>;

public record ProviderConfigurationResponse(
    Guid Id,
    ChannelType ChannelType,
    ProviderType Provider,
    bool IsActive,
    bool IsPrimary,
    int Priority
);
