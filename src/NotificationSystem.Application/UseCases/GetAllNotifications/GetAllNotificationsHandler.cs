using FluentResults;
using MediatR;
using NotificationSystem.Application.Common.Mappings;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetAllNotifications;

public class GetAllNotificationsHandler : IRequestHandler<GetAllNotificationsQuery, Result<GetAllNotificationsResponse>>
{
    public async Task<Result<GetAllNotificationsResponse>> Handle(
        GetAllNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        var domainNotifications = new List<Notification>
        {
            // Notificação com apenas 1 canal (Email)
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                Channels = new List<NotificationChannel>
                {
                    new EmailChannel
                    {
                        Id = Guid.NewGuid(),
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddHours(-2),
                        To = "user@example.com",
                        Subject = "Welcome!",
                        Body = "Welcome to our notification system",
                        IsBodyHtml = false
                    }
                }
            },

            // Notificação multi-canal: Email + SMS
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId2,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Channels = new List<NotificationChannel>
                {
                    new EmailChannel
                    {
                        Id = Guid.NewGuid(),
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddHours(-1),
                        To = "customer@example.com",
                        Subject = "Appointment Reminder",
                        Body = "You have an appointment tomorrow at 2 PM",
                        IsBodyHtml = true
                    },
                    new SmsChannel
                    {
                        Id = Guid.NewGuid(),
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddHours(-1),
                        To = "+5511999999999",
                        Message = "Reminder: Appointment tomorrow at 2 PM",
                        SenderId = "MyApp"
                    }
                }
            },

            // Notificação multi-canal: Email + SMS + Push
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId3,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                Channels = new List<NotificationChannel>
                {
                    new EmailChannel
                    {
                        Id = Guid.NewGuid(),
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddMinutes(-30),
                        To = "vip@example.com",
                        Subject = "Security Alert",
                        Body = "New login detected from unknown device",
                        IsBodyHtml = true
                    },
                    new SmsChannel
                    {
                        Id = Guid.NewGuid(),
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddMinutes(-30),
                        To = "+5511988888888",
                        Message = "Security Alert: New login detected",
                        SenderId = "Security"
                    },
                    new PushChannel
                    {
                        Id = Guid.NewGuid(),
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddMinutes(-30),
                        To = "device-token-456",
                        Content = new NotificationContent
                        {
                            Title = "Security Alert",
                            Body = "New login detected from unknown device",
                            ClickAction = "/security"
                        },
                        IsRead = false,
                        Priority = "high"
                    }
                }
            },

            // Notificação com canal que falhou
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                Channels = new List<NotificationChannel>
                {
                    new SmsChannel
                    {
                        Id = Guid.NewGuid(),
                        Status = NotificationStatus.Failed,
                        ErrorMessage = "Invalid phone number",
                        To = "+invalid",
                        Message = "Your verification code is 123456",
                        SenderId = "MyApp"
                    }
                }
            }
        };

        var notificationDtos = domainNotifications.Select(n => n.ToDto()).ToList();

        var response = new GetAllNotificationsResponse(
            notificationDtos,
            TotalCount: notificationDtos.Count,
            request.PageNumber,
            request.PageSize
        );

        return await Task.FromResult(Result.Ok(response));
    }
}
