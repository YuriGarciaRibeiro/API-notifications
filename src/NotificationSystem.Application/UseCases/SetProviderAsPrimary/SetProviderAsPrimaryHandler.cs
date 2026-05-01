using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.SetProviderAsPrimary;

public class SetProviderAsPrimaryHandler(IProviderConfigurationRepository repository) : IRequestHandler<SetProviderAsPrimaryCommand, Result>
{
    private readonly IProviderConfigurationRepository _repository = repository;

    public async Task<Result> Handle(SetProviderAsPrimaryCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.SetAsPrimaryAsync(request.ProviderId, cancellationToken);
        if (result.IsFailed)
            return result;

        return Result.Ok();
    }
}
