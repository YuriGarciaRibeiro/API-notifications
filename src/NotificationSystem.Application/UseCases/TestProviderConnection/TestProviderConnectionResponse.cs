using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.TestProviderConnection;

public record TestProviderConnectionResponse(
    bool Success,
    ProviderType Provider,
    DateTime CheckedAt,
    string Message,
    Dictionary<string, string> Details
);
