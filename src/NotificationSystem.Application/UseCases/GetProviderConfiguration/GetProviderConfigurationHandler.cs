using FluentResults;
using MediatR;
using NotificationSystem.Application.Common;
using NotificationSystem.Application.Common.Errors;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetProviderConfiguration;

public class GetProviderConfigurationHandler(IProviderConfigurationRepository repository)
    : IRequestHandler<GetProviderConfigurationQuery, Result<ProviderConfigurationDetailsResponse>>
{
    private readonly IProviderConfigurationRepository _repository = repository;

    public async Task<Result<ProviderConfigurationDetailsResponse>> Handle(
        GetProviderConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        var provider = await _repository.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider is null)
            return Result.Fail(new NotFoundError("Provider configuration", request.ProviderId));

        var parsed = ProviderConfigurationSecurityHelper.ParseConfigurationObject(provider.ConfigurationJson);
        var (safeConfiguration, secretConfigured) = ProviderConfigurationSecurityHelper.BuildSafeConfiguration(
            provider.Provider,
            parsed);

        var response = new ProviderConfigurationDetailsResponse(
            provider.Id,
            provider.ChannelType,
            provider.Provider,
            provider.IsActive,
            provider.IsPrimary,
            provider.Priority,
            safeConfiguration,
            secretConfigured);

        return Result.Ok(response);
    }
}
