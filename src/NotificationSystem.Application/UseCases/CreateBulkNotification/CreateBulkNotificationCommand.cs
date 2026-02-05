using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

public record CreateBulkNotificationCommand(
    string Name,
    string? Description,
    List<CreateBulkNotificationItemRequest> Items
) : IRequest<Result<Guid>>;

public record CreateBulkNotificationItemRequest(
    string Recipient,
    ChannelType Channel,
    Dictionary<string, string>? Variables = null
);