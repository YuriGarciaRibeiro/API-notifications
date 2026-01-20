using FluentResults;
using MediatR;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProvider;

public record CreateProviderCommand(
    ChannelType ChannelType,
    ProviderType Provider,
    object Configuration,
    bool IsActive = true,
    bool IsPrimary = false
) : IRequest<Result<Guid>>;
