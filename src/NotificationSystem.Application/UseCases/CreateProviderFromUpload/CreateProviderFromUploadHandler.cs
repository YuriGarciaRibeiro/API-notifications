using FluentResults;
using MediatR;
using NotificationSystem.Application.UseCases.CreateProviderFromFile;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProviderFromUpload;

public class CreateProviderFromUploadHandler(IMediator mediator)
    : IRequestHandler<CreateProviderFromUploadCommand, Result<CreateProviderFromUploadResponse>>
{
    private readonly IMediator _mediator = mediator;

    public async Task<Result<CreateProviderFromUploadResponse>> Handle(CreateProviderFromUploadCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ChannelType>(request.ChannelType, true, out var channelType))
        {
            return Result.Fail<CreateProviderFromUploadResponse>("Invalid channelType");
        }

        if (!Enum.TryParse<ProviderType>(request.Provider, true, out var provider))
        {
            return Result.Fail<CreateProviderFromUploadResponse>("Invalid provider");
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

        var result = await _mediator.Send(fileCommand, cancellationToken);

        if (result.IsFailed)
            return Result.Fail<CreateProviderFromUploadResponse>(result.Errors);

        return Result.Ok(new CreateProviderFromUploadResponse(result.Value.ProviderId));
    }
}
