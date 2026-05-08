using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllProviders;

public class GetAllProvidersHandler(IProviderConfigurationRepository repository) : IRequestHandler<GetAllProvidersQuery, Result<GetAllProvidersResponse>>
{
    private readonly IProviderConfigurationRepository _repository = repository;

    public async Task<Result<GetAllProvidersResponse>> Handle(
        GetAllProvidersQuery request,
        CancellationToken cancellationToken)
    {
        var providers = await _repository.GetAllProvidersAsync(cancellationToken);

        // Filtra por tipo de canal se especificado
        if (request.ChannelType.HasValue)
        {
            providers = providers.Where(p => p.ChannelType == request.ChannelType.Value).ToList();
        }

        var responseItems = providers.Select(p => new ProviderConfigurationResponse(
            p.Id,
            p.ChannelType,
            p.Provider,
            p.IsActive,
            p.IsPrimary,
            p.Priority
        )).ToList();

        return Result.Ok(new GetAllProvidersResponse(responseItems));
    }
}
