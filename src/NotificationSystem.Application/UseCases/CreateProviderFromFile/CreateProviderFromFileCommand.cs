using FluentResults;
using MediatR;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProviderFromFile;

/// <summary>
/// Comando para criar um provedor a partir de upload de arquivo de credenciais
/// </summary>
public record CreateProviderFromFileCommand(
    ChannelType ChannelType,
    ProviderType Provider,
    Stream FileStream,
    string FileName,
    long FileSize,
    string? ProjectId = null,
    bool IsActive = true,
    bool IsPrimary = false
) : IRequest<Result<Guid>>;
