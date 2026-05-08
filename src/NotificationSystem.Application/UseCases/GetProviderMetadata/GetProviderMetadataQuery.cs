using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetProviderMetadata;

public record GetProviderMetadataQuery() : IRequest<Result<ProviderMetadataResponse>>;
