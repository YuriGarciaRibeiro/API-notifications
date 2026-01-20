using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.DeleteProvider;

public class DeleteProviderHandler(IProviderConfigurationRepository repository) : IRequestHandler<DeleteProviderCommand>
{
    public readonly IProviderConfigurationRepository _repository = repository;

    public Task Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        return _repository.DeleteAsync(request.Id, cancellationToken);
    }
}