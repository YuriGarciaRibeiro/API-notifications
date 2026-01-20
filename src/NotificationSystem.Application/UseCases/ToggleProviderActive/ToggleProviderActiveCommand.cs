using MediatR;

namespace NotificationSystem.Application.UseCases.ToggleProviderActive;

public record ToggleProviderActiveCommand(Guid ProviderId) : IRequest;