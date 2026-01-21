using NotificationSystem.Application.DTOs.Notifications;

namespace NotificationSystem.Application.UseCases.GetAllNotifications;

public record GetAllNotificationsResponse(
    IEnumerable<NotificationDto> Notifications,
    int TotalCount,
    int PageNumber,
    int PageSize
);
