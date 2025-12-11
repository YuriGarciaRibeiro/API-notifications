using NotificationSystem.Application.UseCases.GetAllNotifications;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Common.Mappings;

public static class NotificationMappings
{
    /// <summary>
    /// Mapeia uma Notification do domínio para o DTO polimórfico apropriado
    /// </summary>
    public static NotificationDto ToDto(this Notification notification)
    {
        return notification switch
        {
            EmailNotification email => new EmailNotificationDto
            {
                Id = email.Id,
                UserId = email.UserId,
                CreatedAt = email.CreatedAt,
                Status = email.Status,
                ErrorMessage = email.ErrorMessage,
                SentAt = email.SentAt,
                To = email.To,
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = email.IsBodyHtml
            },

            SmsNotification sms => new SmsNotificationDto
            {
                Id = sms.Id,
                UserId = sms.UserId,
                CreatedAt = sms.CreatedAt,
                Status = sms.Status,
                ErrorMessage = sms.ErrorMessage,
                SentAt = sms.SentAt,
                To = sms.To,
                Message = sms.Message,
                SenderId = sms.SenderId
            },

            PushNotification push => new PushNotificationDto
            {
                Id = push.Id,
                UserId = push.UserId,
                CreatedAt = push.CreatedAt,
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

            _ => throw new InvalidOperationException($"Unknown notification type: {notification.GetType().Name}")
        };
    }
}
