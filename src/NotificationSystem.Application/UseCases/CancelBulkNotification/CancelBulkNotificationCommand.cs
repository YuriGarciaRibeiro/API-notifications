using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.CancelBulkNotification;

public record CancelBulkNotificationCommand(Guid JobId) : IRequest<Result>;
