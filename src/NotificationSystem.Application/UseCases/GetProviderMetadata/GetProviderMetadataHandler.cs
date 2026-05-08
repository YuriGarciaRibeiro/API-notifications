using FluentResults;
using MediatR;
using NotificationSystem.Application.Common;

namespace NotificationSystem.Application.UseCases.GetProviderMetadata;

public class GetProviderMetadataHandler : IRequestHandler<GetProviderMetadataQuery, Result<ProviderMetadataResponse>>
{
    public Task<Result<ProviderMetadataResponse>> Handle(
        GetProviderMetadataQuery request,
        CancellationToken cancellationToken)
    {
        var metadata = ProviderMetadataCatalog.Build();
        return Task.FromResult(Result.Ok(metadata));
    }
}
