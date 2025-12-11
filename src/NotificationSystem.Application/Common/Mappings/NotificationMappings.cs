using NotificationSystem.Application.UseCases.GetAllNotifications;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Common.Mappings;

public static class NotificationMappings
{
    /// <summary>
    /// Mapeia uma Notification do domínio para o DTO
    /// </summary>
    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            CreatedAt = notification.CreatedAt,
            Channels = notification.Channels.Select(c => c.ToDto()).ToList()
        };
    }

    /// <summary>
    /// Mapeia um NotificationChannel do domínio para o DTO polimórfico apropriado
    /// </summary>
    public static ChannelDto ToDto(this NotificationChannel channel)
    {
        return channel switch
        {
            EmailChannel email => new EmailChannelDto
            {
                Id = email.Id,
                Status = email.Status,
                ErrorMessage = email.ErrorMessage,
                SentAt = email.SentAt,
                To = email.To,
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = email.IsBodyHtml
            },

            SmsChannel sms => new SmsChannelDto
            {
                Id = sms.Id,
                Status = sms.Status,
                ErrorMessage = sms.ErrorMessage,
                SentAt = sms.SentAt,
                To = sms.To,
                Message = sms.Message,
                SenderId = sms.SenderId
            },

            PushChannel push => new PushChannelDto
            {
                Id = push.Id,
                Status = push.Status,
                ErrorMessage = push.ErrorMessage,
                SentAt = push.SentAt,
                To = push.To,
                Content = new NotificationContentDto
                {
                    Title = push.Content.Title,
                    Body = push.Content.Body,
                    ClickAction = push.Content.ClickAction
                },
                Data = push.Data,
                Priority = push.Priority,
                TimeToLive = push.TimeToLive,
                IsRead = push.IsRead
            },

            _ => throw new InvalidOperationException($"Unknown channel type: {channel.GetType().Name}")
        };
    }
}
