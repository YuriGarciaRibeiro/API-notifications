using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.ToggleProviderActive;

public class ToggleProviderActiveHandler(IProviderConfigurationRepository repository) : IRequestHandler<ToggleProviderActiveCommand, Result>
{
    private readonly IProviderConfigurationRepository _repository = repository;

    public async Task<Result> Handle(ToggleProviderActiveCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.ToggleActiveStatusAsync(request.ProviderId, cancellationToken);
        if (result.IsFailed)
            return result;

        return Result.Ok();
    }
}
