using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Notifications;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAllNotifications;

public class GetAllNotificationsHandler(INotificationRepository notificationRepository) : IRequestHandler<GetAllNotificationsQuery, Result<GetAllNotificationsResponse>>
{

    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<Result<GetAllNotificationsResponse>> Handle(
        GetAllNotificationsQuery request,
        CancellationToken cancellationToken)
    {

        var (pageNumber, pageSize) = request;

        var notifications = await _notificationRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
        var totalCount = await _notificationRepository.GetTotalCountAsync(cancellationToken);

        var notificationsList = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            CreatedAt = n.CreatedAt,
            Origin = n.Origin,
            Type = n.Type,
            Channels = [.. n.Channels.Select<NotificationChannel, ChannelDto>(c =>
            {
                return c switch
                {
                    EmailChannel emailChannel => new EmailChannelDto
                    {
                        Id = emailChannel.Id,
                        Status = emailChannel.Status,
                        ErrorMessage = emailChannel.ErrorMessage,
                        SentAt = emailChannel.SentAt,
                        To = emailChannel.To,
                        Subject = emailChannel.Subject,
                        Body = emailChannel.Body,
                        IsBodyHtml = emailChannel.IsBodyHtml
                    },
                    SmsChannel smsChannel => new SmsChannelDto
                    {
                        Id = smsChannel.Id,
                        Status = smsChannel.Status,
                        ErrorMessage = smsChannel.ErrorMessage,
                        SentAt = smsChannel.SentAt,
                        To = smsChannel.To,
                        Message = smsChannel.Message,
                        SenderId = smsChannel.SenderId
                    },
                    PushChannel pushChannel => new PushChannelDto
                    {
                        Id = pushChannel.Id,
                        Status = pushChannel.Status,
                        ErrorMessage = pushChannel.ErrorMessage,
                        SentAt = pushChannel.SentAt,
                        To = pushChannel.To,
                        Content = new NotificationContentDto
                        {
                            Title = pushChannel.Content.Title,
                            Body = pushChannel.Content.Body
                        },
                        Data = pushChannel.Data,
                        Priority = pushChannel.Priority,
                        TimeToLive = pushChannel.TimeToLive,
                        IsRead = pushChannel.IsRead
                    },
                    _ => throw new NotSupportedException("Canal de notificação desconhecido")
                };
            })]
        }).ToList();

        var response = new GetAllNotificationsResponse(
            notificationsList,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize);

        return Result.Ok(response);
    }
}
