using System.Text.Json;
using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProvider;

public class CreateProviderHandler : IRequestHandler<CreateProviderCommand, Result<Guid>>
{
    private readonly IProviderConfigurationRepository _repository;

    public CreateProviderHandler(IProviderConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        // Serializa a configuração para JSON
        var configJson = JsonSerializer.Serialize(request.Configuration);

        var providerConfig = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            ChannelType = request.ChannelType,
            Provider = request.Provider,
            ConfigurationJson = configJson,
            IsActive = request.IsActive,
            IsPrimary = request.IsPrimary
        };

        await _repository.CreateAsync(providerConfig);

        return Result.Ok(providerConfig.Id);
    }
}
