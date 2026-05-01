using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.DeleteProvider;

public class DeleteProviderHandler(IProviderConfigurationRepository repository) : IRequestHandler<DeleteProviderCommand, Result>
{
    private readonly IProviderConfigurationRepository _repository = repository;

    public async Task<Result> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.DeleteAsync(request.Id, cancellationToken);
        if (result.IsFailed)
            return result;

        return Result.Ok();
    }
}
