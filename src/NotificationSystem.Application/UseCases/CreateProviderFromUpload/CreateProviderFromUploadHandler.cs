using FluentResults;
using MediatR;
using NotificationSystem.Application.UseCases.CreateProviderFromFile;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProviderFromUpload;

public class CreateProviderFromUploadHandler(IMediator mediator)
    : IRequestHandler<CreateProviderFromUploadCommand, Result<Guid>>
{
    private readonly IMediator _mediator = mediator;

    public Task<Result<Guid>> Handle(CreateProviderFromUploadCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ChannelType>(request.ChannelType, true, out var channelType))
        {
            return Task.FromResult(Result.Fail<Guid>("Invalid channelType"));
        }

        if (!Enum.TryParse<ProviderType>(request.Provider, true, out var provider))
        {
            return Task.FromResult(Result.Fail<Guid>("Invalid provider"));
        }

        var isActive = !bool.TryParse(request.IsActive, out var parsedIsActive) || parsedIsActive;
        var isPrimary = bool.TryParse(request.IsPrimary, out var parsedIsPrimary) && parsedIsPrimary;

        var fileCommand = new CreateProviderFromFileCommand(
            channelType,
            provider,
            request.FileStream ?? Stream.Null,
            request.FileName ?? string.Empty,
            request.FileSize,
            request.ProjectId,
            isActive,
            isPrimary);

        return _mediator.Send(fileCommand, cancellationToken);
    }
}
