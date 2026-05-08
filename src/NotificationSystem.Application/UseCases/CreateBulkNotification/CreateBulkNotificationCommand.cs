using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

public record CreateBulkNotificationCommand(
    string Name,
    string? Description,
    List<CreateBulkNotificationItemRequest> Items,
    DateTime? ScheduledFor = null,
    string? RecurringCron = null,
    string? TimeZone = null
) : IRequest<Result<CreateBulkNotificationResponse>>;

public record CreateBulkNotificationItemRequest(
    string Recipient,
    ChannelType Channel,
    Dictionary<string, string>? Variables = null
);
