using FluentResults;
using MediatR;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.SetProviderAsPrimary;

public record SetProviderAsPrimaryCommand(
    Guid ProviderId,
    ChannelType ChannelType
) : IRequest<Result>;
