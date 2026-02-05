using FluentResults;
using MediatR;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetBulkNotificationItems;

public record GetBulkNotificationItemsQuery(Guid JobId, NotificationStatus? Status = null)
    : IRequest<Result<IEnumerable<BulkNotificationItemResponse>>>;

public record BulkNotificationItemResponse(
    Guid Id,
    string Recipient,
    string Channel,
    string Status,
    string? ErrorMessage,
    DateTime? SentAt,
    DateTime CreatedAt
);
