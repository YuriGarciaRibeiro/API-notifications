using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetProviderConfiguration;

public record GetProviderConfigurationQuery(Guid ProviderId) : IRequest<Result<ProviderConfigurationDetailsResponse>>;
