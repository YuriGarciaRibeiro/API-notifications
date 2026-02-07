using System.Text.Json;
using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.CreateProvider;

public class CreateProviderHandler(IProviderConfigurationRepository repository) : IRequestHandler<CreateProviderCommand, Result<Guid>>
{
    private readonly IProviderConfigurationRepository _repository = repository;

    public async Task<Result<Guid>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        // Serializa a configuração para JSON
        var configJson = JsonSerializer.Serialize(request.Configuration);

        // Verifica se é o primeiro provider deste canal
        var hasExistingProvider = await _repository.HasAnyProviderForChannelAsync(request.ChannelType, cancellationToken);
        var isPrimary = request.IsPrimary || !hasExistingProvider;

        var providerConfig = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            ChannelType = request.ChannelType,
            Provider = request.Provider,
            ConfigurationJson = configJson,
            IsActive = request.IsActive,
            IsPrimary = isPrimary
        };

        await _repository.CreateAsync(providerConfig, cancellationToken);

        return Result.Ok(providerConfig.Id);
    }
}
