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
        var domainNotifications = new List<Notification>
        {
            new EmailNotification
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow,
                To = "user@example.com",
                Subject = "Welcome!",
                Body = "Welcome to our notification system",
                IsBodyHtml = false
            },
            new SmsNotification
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Pending,
                To = "+5511999999999",
                Message = "Your code is 123456",
                SenderId = "MyApp"
            },
            new PushNotification
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow,
                To = "device-token-123",
                Content = new NotificationContent
                {
                    Title = "New Message",
                    Body = "You have a new message",
                    ClickAction = "/messages"
                },
                IsRead = false
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
