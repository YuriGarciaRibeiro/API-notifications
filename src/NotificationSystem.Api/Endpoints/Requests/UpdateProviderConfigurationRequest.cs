using System.Text.Json;

namespace NotificationSystem.Api.Endpoints.Requests;

public sealed record UpdateProviderConfigurationRequest(
    JsonElement Configuration
);
