using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using FcmAndroidConfig = FirebaseAdmin.Messaging.AndroidConfig;
using FcmApnsConfig = FirebaseAdmin.Messaging.ApnsConfig;
using FcmWebpushConfig = FirebaseAdmin.Messaging.WebpushConfig;
using FcmNotification = FirebaseAdmin.Messaging.Notification;

namespace NotificationSystem.Application.Services;

public class FirebaseService : IPushNotificationService, IDisposable
{
    private readonly FirebaseApp _firebaseApp;
    private bool _disposed;

    /// <summary>
    /// Construtor que cria uma instância do FirebaseApp a partir de um arquivo de credenciais
    /// </summary>
    public FirebaseService(string credentialsPath, string projectId)
    {
        if (string.IsNullOrEmpty(credentialsPath))
            throw new ArgumentNullException(nameof(credentialsPath));

        if (!File.Exists(credentialsPath))
            throw new FileNotFoundException($"Firebase credentials file not found at: {credentialsPath}");

        _firebaseApp = FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(credentialsPath),
            ProjectId = projectId
        }, $"firebase-{Guid.NewGuid()}"); // Nome único para cada instância
    }

    /// <summary>
    /// Construtor que cria uma instância do FirebaseApp a partir do conteúdo JSON das credenciais
    /// </summary>
    public FirebaseService(string credentialsJson)
    {
        if (string.IsNullOrEmpty(credentialsJson))
            throw new ArgumentNullException(nameof(credentialsJson));

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialsJson));

        _firebaseApp = FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromStream(stream)
        }, $"firebase-{Guid.NewGuid()}"); // Nome único para cada instância
    }

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

        return await FirebaseMessaging.GetMessaging(_firebaseApp).SendAsync(message);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _firebaseApp?.Delete();
            _disposed = true;
        }
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
