using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.TestProviderConnection;

public record TestProviderConnectionQuery(Guid ProviderId)
    : IRequest<Result<TestProviderConnectionResponse>>;
