using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Notifications;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetNotificationById;

public class GetNotificationByIdHandler(INotificationRepository notificationRepository)
    : IRequestHandler<GetNotificationByIdQuery, Result<GetNotificationByIdResponse>>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<Result<GetNotificationByIdResponse>> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.Id);

        if (notification is null)
        {
            var error = new Error($"Notification with id '{request.Id}' was not found.")
                .WithMetadata("StatusCode", 404);
            return Result.Fail(error);
        }

        var response = new GetNotificationByIdResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            CreatedAt = notification.CreatedAt,
            Origin = notification.Origin,
            Type = notification.Type,
            Channels = [.. notification.Channels.Select<NotificationChannel, ChannelDto>(c =>
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
                            Body = pushChannel.Content.Body,
                            ClickAction = pushChannel.Content.ClickAction
                        },
                        Data = pushChannel.Data,
                        Priority = pushChannel.Priority,
                        TimeToLive = pushChannel.TimeToLive,
                        IsRead = pushChannel.IsRead
                    },
                    _ => throw new NotSupportedException("Unknown notification channel type")
                };
            })]
        };

        return Result.Ok(response);
    }
}
