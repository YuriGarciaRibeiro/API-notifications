using FluentResults;
using MediatR;
using NotificationSystem.Application.Contracts.Notifications;

namespace NotificationSystem.Application.UseCases.CreateNotification;

public record CreateNotificationCommand(
    Guid UserId,
    List<ChannelRequest> Channels
) : IRequest<Result<CreateNotificationResponse>>;
