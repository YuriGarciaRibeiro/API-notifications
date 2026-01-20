using MediatR;

namespace NotificationSystem.Application.UseCases.DeleteProvider;

public record DeleteProviderCommand(Guid Id) : IRequest;