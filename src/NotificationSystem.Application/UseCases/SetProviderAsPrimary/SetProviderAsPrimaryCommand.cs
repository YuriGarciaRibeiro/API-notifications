using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.SetProviderAsPrimary;

public record SetProviderAsPrimaryCommand(
    Guid ProviderId
) : IRequest<Result>;
