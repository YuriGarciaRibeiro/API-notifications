using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.SetProviderAsPrimary;

public class SetProviderAsPrimaryHandler : IRequestHandler<SetProviderAsPrimaryCommand, Result>
{
    private readonly IProviderConfigurationRepository _repository;

    public SetProviderAsPrimaryHandler(IProviderConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SetProviderAsPrimaryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.SetAsPrimaryAsync(request.ProviderId);
            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
