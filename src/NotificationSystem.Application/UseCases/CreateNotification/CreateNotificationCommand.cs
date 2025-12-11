using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.CreateNotification;

public record CreateNotificationCommand(
    Guid UserId,
    List<ChannelRequest> Channels
) : IRequest<Result<Guid>>;

public record ChannelRequest(
    string Type,
    Dictionary<string, object> Data
);
