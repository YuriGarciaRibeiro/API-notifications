using FluentResults;
using MediatR;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateNotification;

public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    private readonly INotificationRepository _repository;

    public CreateNotificationHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            Channels = []
        };

        foreach (var channelRequest in request.Channels)
        {
            NotificationChannel? channel = channelRequest.Type.ToLower() switch
            {
                "email" => CreateEmailChannel(notification.Id, channelRequest.Data),
                "sms" => CreateSmsChannel(notification.Id, channelRequest.Data),
                "push" => CreatePushChannel(notification.Id, channelRequest.Data),
                _ => null
            };

            if (channel != null)
            {
                notification.Channels.Add(channel);
            }
        }

        await _repository.AddAsync(notification);

        notification.PublishToAllChannels();

        await _repository.UpdateAsync(notification);

        return Result.Ok(notification.Id);
    }

    private static EmailChannel CreateEmailChannel(Guid notificationId, Dictionary<string, object> data)
    {
        return new EmailChannel
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            To = data.GetValueOrDefault("to")?.ToString() ?? string.Empty,
            Subject = data.GetValueOrDefault("subject")?.ToString() ?? string.Empty,
            Body = data.GetValueOrDefault("body")?.ToString() ?? string.Empty,
            IsBodyHtml = bool.TryParse(data.GetValueOrDefault("isBodyHtml")?.ToString(), out var isHtml) && isHtml
        };
    }

    private static SmsChannel CreateSmsChannel(Guid notificationId, Dictionary<string, object> data)
    {
        return new SmsChannel
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            To = data.GetValueOrDefault("to")?.ToString() ?? string.Empty,
            Message = data.GetValueOrDefault("message")?.ToString() ?? string.Empty,
            SenderId = data.GetValueOrDefault("senderId")?.ToString()
        };
    }

    private static PushChannel CreatePushChannel(Guid notificationId, Dictionary<string, object> data)
    {
        return new PushChannel
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            To = data.GetValueOrDefault("to")?.ToString() ?? string.Empty,
            Content = new NotificationContent
            {
                Title = data.GetValueOrDefault("title")?.ToString() ?? string.Empty,
                Body = data.GetValueOrDefault("body")?.ToString() ?? string.Empty,
                ClickAction = data.GetValueOrDefault("clickAction")?.ToString()
            },
            Priority = data.GetValueOrDefault("priority")?.ToString()
        };
    }
}
