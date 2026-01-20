using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProviderFromFile;

/// <summary>
/// Comando para criar um provedor a partir de upload de arquivo de credenciais
/// </summary>
public record CreateProviderFromFileCommand(
    ChannelType ChannelType,
    ProviderType Provider,
    IFormFile CredentialsFile,
    string? ProjectId = null,
    bool IsActive = true,
    bool IsPrimary = false
) : IRequest<Result<Guid>>;
