using System.Text.Json;
using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.UpdateProviderConfiguration;

public record UpdateProviderConfigurationCommand(
    Guid ProviderId,
    JsonElement Configuration
) : IRequest<Result>;
