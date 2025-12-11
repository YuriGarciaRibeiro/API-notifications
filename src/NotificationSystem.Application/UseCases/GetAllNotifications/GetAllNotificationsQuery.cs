using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetAllNotifications;

public record GetAllNotificationsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<GetAllNotificationsResponse>>;
