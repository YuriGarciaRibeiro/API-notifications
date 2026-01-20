using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.ToggleProviderActive;

public class ToggleProviderActiveHandler(IProviderConfigurationRepository repository) : IRequestHandler<ToggleProviderActiveCommand>
{

    private readonly IProviderConfigurationRepository _repository = repository;

    public Task Handle(ToggleProviderActiveCommand request, CancellationToken cancellationToken)
    {
        return _repository.ToggleActiveStatusAsync(request.ProviderId, cancellationToken);
    }
}