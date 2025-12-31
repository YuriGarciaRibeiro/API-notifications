using FirebaseAdmin.Messaging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using FcmAndroidConfig = FirebaseAdmin.Messaging.AndroidConfig;
using FcmApnsConfig = FirebaseAdmin.Messaging.ApnsConfig;
using FcmWebpushConfig = FirebaseAdmin.Messaging.WebpushConfig;
using FcmNotification = FirebaseAdmin.Messaging.Notification;

namespace NotificationSystem.Application.Services;

public class FirebaseService : IPushNotificationService
{
    public async Task<string> SendPushNotificationAsync(PushChannelMessage pushMessage)
    {
        var message = new Message
        {
            Token = pushMessage.To,
            Notification = new FcmNotification
            {
                Title = pushMessage.Content.Title,
                Body = pushMessage.Content.Body,
            },
            Data = pushMessage.Data,
            Android = MapAndroidConfig(pushMessage.Android, pushMessage.Content),
            Apns = MapApnsConfig(pushMessage.Apns, pushMessage.Content),
            Webpush = MapWebpushConfig(pushMessage.Webpush, pushMessage.Content)
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    private static FcmAndroidConfig? MapAndroidConfig(AndroidConfigMessage? androidConfig, PushContentMessage content)
    {
        if (androidConfig == null) return null;

        var config = new FcmAndroidConfig
        {
            Priority = androidConfig.Priority switch
            {
                "high" => Priority.High,
                "normal" => Priority.Normal,
                _ => null
            },
            TimeToLive = androidConfig.Ttl != null ? TimeSpan.Parse(androidConfig.Ttl) : null,
            CollapseKey = androidConfig.CollapseKey,
            RestrictedPackageName = androidConfig.RestrictedPackageName
        };

        if (androidConfig.Notification != null)
        {
            var androidNotification = new AndroidNotification
            {
                Title = androidConfig.Notification.Title ?? content.Title,
                Body = androidConfig.Notification.Body ?? content.Body,
                Icon = androidConfig.Notification.Icon,
                Color = androidConfig.Notification.Color,
                Sound = androidConfig.Notification.Sound,
                Tag = androidConfig.Notification.Tag,
                ClickAction = androidConfig.Notification.ClickAction ?? content.ClickAction,
                BodyLocKey = androidConfig.Notification.BodyLocKey,
                BodyLocArgs = androidConfig.Notification.BodyLocArgs,
                TitleLocKey = androidConfig.Notification.TitleLocKey,
                TitleLocArgs = androidConfig.Notification.TitleLocArgs,
                ChannelId = androidConfig.Notification.ChannelId,
                Ticker = androidConfig.Notification.Ticker,
                Sticky = androidConfig.Notification.Sticky ?? false,
                LocalOnly = androidConfig.Notification.LocalOnly ?? false,
                Priority = androidConfig.Notification.NotificationPriority.HasValue
                    ? (NotificationPriority)androidConfig.Notification.NotificationPriority.Value
                    : null,
                NotificationCount = androidConfig.Notification.NotificationCount,
                ImageUrl = androidConfig.Notification.ImageUrl
            };

            if (androidConfig.Notification.EventTime != null)
            {
                androidNotification.EventTimestamp = DateTime.Parse(androidConfig.Notification.EventTime);
            }

            config.Notification = androidNotification;
        }

        return config;
    }

    private static FcmApnsConfig? MapApnsConfig(ApnsConfigMessage? apnsConfig, PushContentMessage content)
    {
        if (apnsConfig == null) return null;

        var config = new FcmApnsConfig
        {
            Headers = apnsConfig.Headers ?? []
        };

        return config;
    }

    private static FcmWebpushConfig? MapWebpushConfig(WebpushConfigMessage? webpushConfig, PushContentMessage content)
    {
        if (webpushConfig == null) return null;

        var config = new FcmWebpushConfig
        {
            Headers = webpushConfig.Headers ?? []
        };

        if (webpushConfig.FcmOptions != null)
        {
            config.FcmOptions = new WebpushFcmOptions
            {
                Link = webpushConfig.FcmOptions.Link
            };
        }

        return config;
    }
}
