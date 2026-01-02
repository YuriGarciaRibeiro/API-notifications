using FluentResults;
using MediatR;
using NotificationSystem.Application.Interfaces;
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
        var to = GetStringValue(data, "to");
        var subject = GetStringValue(data, "subject");
        var body = GetStringValue(data, "body");
        var isBodyHtml = GetBoolValue(data, "isBodyHtml");

        return new EmailChannel
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            To = to,
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };
    }

    private static string GetStringValue(Dictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value == null)
            return string.Empty;

        // Handle JsonElement from System.Text.Json
        if (value is System.Text.Json.JsonElement jsonElement)
        {
            return jsonElement.ValueKind == System.Text.Json.JsonValueKind.String
                ? jsonElement.GetString() ?? string.Empty
                : jsonElement.ToString();
        }

        return value.ToString() ?? string.Empty;
    }

    private static bool GetBoolValue(Dictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value == null)
            return false;

        // Handle JsonElement from System.Text.Json
        if (value is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.True)
                return true;
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.False)
                return false;
        }

        return bool.TryParse(value.ToString(), out var result) && result;
    }

    private static bool? GetNullableBoolValue(Dictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value == null)
            return null;

        if (value is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.True)
                return true;
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.False)
                return false;
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Null)
                return null;
        }

        return bool.TryParse(value.ToString(), out var result) ? result : null;
    }

    private static int? GetIntValue(Dictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value == null)
            return null;

        if (value is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                return jsonElement.GetInt32();
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Null)
                return null;
        }

        return int.TryParse(value.ToString(), out var result) ? result : null;
    }

    private static Dictionary<string, string> GetStringDictionaryValue(Dictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value == null)
            return [];

        if (value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in jsonElement.EnumerateObject())
            {
                dict[prop.Name] = prop.Value.ToString();
            }
            return dict;
        }

        return [];
    }

    private static Dictionary<string, object>? GetObjectValue(Dictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value == null)
            return null;

        if (value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in jsonElement.EnumerateObject())
            {
                dict[prop.Name] = prop.Value;
            }
            return dict;
        }

        return null;
    }

    private static SmsChannel CreateSmsChannel(Guid notificationId, Dictionary<string, object> data)
    {
        return new SmsChannel
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            To = GetStringValue(data, "to"),
            Message = GetStringValue(data, "message"),
            SenderId = string.IsNullOrEmpty(GetStringValue(data, "senderId"))
                ? null
                : GetStringValue(data, "senderId")
        };
    }

    private static PushChannel CreatePushChannel(Guid notificationId, Dictionary<string, object> data)
    {
        return new PushChannel
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            To = GetStringValue(data, "to"),
            Content = new NotificationContent
            {
                Title = GetStringValue(data, "title"),
                Body = GetStringValue(data, "body"),
                ClickAction = GetNullableStringValue(data, "clickAction")
            },
            Data = GetStringDictionaryValue(data, "data"),
            Platform = GetStringValue(data, "platform"),
            Priority = GetNullableStringValue(data, "priority"),
            TimeToLive = GetIntValue(data, "timeToLive"),
            Condition = GetNullableStringValue(data, "condition"),
            MutableContent = GetNullableBoolValue(data, "mutableContent"),
            ContentAvailable = GetNullableBoolValue(data, "contentAvailable"),
            Android = CreateAndroidConfig(GetObjectValue(data, "android")),
            Apns = CreateApnsConfig(GetObjectValue(data, "apns")),
            Webpush = CreateWebpushConfig(GetObjectValue(data, "webpush"))
        };
    }

    private static string? GetNullableStringValue(Dictionary<string, object> data, string key)
    {
        var value = GetStringValue(data, key);
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static AndroidConfig? CreateAndroidConfig(Dictionary<string, object>? data)
    {
        if (data == null) return null;

        return new AndroidConfig
        {
            Priority = GetNullableStringValue(data, "priority"),
            Ttl = GetNullableStringValue(data, "ttl")
        };
    }

    private static ApnsConfig? CreateApnsConfig(Dictionary<string, object>? data)
    {
        if (data == null) return null;

        return new ApnsConfig
        {
            Headers = GetStringDictionaryValue(data, "headers")
        };
    }

    private static WebpushConfig? CreateWebpushConfig(Dictionary<string, object>? data)
    {
        if (data == null) return null;

        return new WebpushConfig
        {
            Headers = GetStringDictionaryValue(data, "headers")
        };
    }
}
