using FluentResults;
using MediatR;
using NotificationSystem.Application.Common;
using NotificationSystem.Application.Common.Errors;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.UpdateProviderConfiguration;

public class UpdateProviderConfigurationHandler(IProviderConfigurationRepository repository)
    : IRequestHandler<UpdateProviderConfigurationCommand, Result>
{
    private readonly IProviderConfigurationRepository _repository = repository;

    public async Task<Result> Handle(UpdateProviderConfigurationCommand request, CancellationToken cancellationToken)
    {
        var provider = await _repository.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider is null)
            return Result.Fail(new NotFoundError("Provider configuration", request.ProviderId));

        var existingConfiguration = ProviderConfigurationSecurityHelper.ParseConfigurationObject(provider.ConfigurationJson);
        var incomingConfiguration = ProviderConfigurationSecurityHelper.ParseConfigurationObject(request.Configuration);

        var mergedConfiguration = ProviderConfigurationSecurityHelper.MergeWithSecretPreservation(
            provider.Provider,
            existingConfiguration,
            incomingConfiguration);

        provider.ConfigurationJson = mergedConfiguration.ToJsonString();

        await _repository.UpdateAsync(provider, cancellationToken);
        return Result.Ok();
    }
}
