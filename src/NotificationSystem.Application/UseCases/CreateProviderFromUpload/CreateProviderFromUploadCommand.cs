using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.CreateProviderFromUpload;

public record CreateProviderFromUploadCommand(
    string? ChannelType,
    string? Provider,
    Stream? FileStream,
    string? FileName,
    long FileSize,
    string? ProjectId = null,
    string? IsActive = null,
    string? IsPrimary = null
) : IRequest<Result<Guid>>;
