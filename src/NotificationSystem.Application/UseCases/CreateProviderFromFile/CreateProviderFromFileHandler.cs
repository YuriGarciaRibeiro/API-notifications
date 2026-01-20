using System.Text.Json;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Settings;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProviderFromFile;

public class CreateProviderFromFileHandler : IRequestHandler<CreateProviderFromFileCommand, Result<Guid>>
{
    private readonly IProviderConfigurationRepository _repository;
    private readonly ILogger<CreateProviderFromFileHandler> _logger;

    public CreateProviderFromFileHandler(
        IProviderConfigurationRepository repository,
        ILogger<CreateProviderFromFileHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateProviderFromFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Lê o conteúdo do arquivo
            using var streamReader = new StreamReader(request.FileStream);
            var fileContent = await streamReader.ReadToEndAsync(cancellationToken);

            // Valida se é um JSON válido
            JsonDocument? jsonDocument = null;
            try
            {
                jsonDocument = JsonDocument.Parse(fileContent);
            }
            catch (JsonException)
            {
                return Result.Fail("Invalid JSON file format");
            }

            // Validações específicas por provedor
            var validationResult = request.Provider switch
            {
                ProviderType.Firebase => ValidateFirebaseCredentials(jsonDocument, request.ProjectId),
                _ => Result.Fail($"File upload not supported for provider type: {request.Provider}")
            };

            if (validationResult.IsFailed)
                return validationResult;

            // Cria a configuração baseada no provedor
            var configuration = request.Provider switch
            {
                ProviderType.Firebase => CreateFirebaseConfiguration(fileContent, request.ProjectId, jsonDocument),
                _ => throw new NotSupportedException($"Provider {request.Provider} is not supported")
            };

            var configJson = JsonSerializer.Serialize(configuration);

            var providerConfig = new ProviderConfiguration
            {
                Id = Guid.NewGuid(),
                ChannelType = request.ChannelType,
                Provider = request.Provider,
                ConfigurationJson = configJson,
                IsActive = request.IsActive,
                isPrimary = request.IsPrimary
            };

            await _repository.CreateAsync(providerConfig);

            _logger.LogInformation(
                "Provider created from file upload. Provider: {Provider}, Channel: {Channel}, Id: {Id}",
                request.Provider, request.ChannelType, providerConfig.Id);

            return Result.Ok(providerConfig.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider from file upload");
            return Result.Fail($"Error processing file: {ex.Message}");
        }
    }

    private static Result ValidateFirebaseCredentials(JsonDocument jsonDocument, string? projectId)
    {
        var root = jsonDocument.RootElement;

        // Valida campos obrigatórios do Firebase
        if (!root.TryGetProperty("type", out var typeProperty) ||
            typeProperty.GetString() != "service_account")
        {
            return Result.Fail("Invalid Firebase credentials: 'type' must be 'service_account'");
        }

        if (!root.TryGetProperty("project_id", out var projectIdProperty))
        {
            return Result.Fail("Invalid Firebase credentials: missing 'project_id'");
        }

        if (!root.TryGetProperty("private_key", out _))
        {
            return Result.Fail("Invalid Firebase credentials: missing 'private_key'");
        }

        if (!root.TryGetProperty("client_email", out _))
        {
            return Result.Fail("Invalid Firebase credentials: missing 'client_email'");
        }

        // Se projectId foi fornecido, valida se bate com o do arquivo
        if (!string.IsNullOrEmpty(projectId))
        {
            var fileProjectId = projectIdProperty.GetString();
            if (fileProjectId != projectId)
            {
                return Result.Fail(
                    $"Project ID mismatch: provided '{projectId}' but file contains '{fileProjectId}'");
            }
        }

        return Result.Ok();
    }

    private static FirebaseSettings CreateFirebaseConfiguration(
        string credentialsJson,
        string? projectId,
        JsonDocument jsonDocument)
    {
        var root = jsonDocument.RootElement;
        var fileProjectId = root.GetProperty("project_id").GetString() ?? string.Empty;

        return new FirebaseSettings
        {
            CredentialsJson = credentialsJson,
            ProjectId = projectId ?? fileProjectId
        };
    }
}
