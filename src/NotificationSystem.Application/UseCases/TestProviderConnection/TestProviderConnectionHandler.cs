using System.Text.Json.Nodes;
using FluentResults;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Common;
using NotificationSystem.Application.Common.Errors;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;
using Twilio;
using Twilio.Exceptions;
using Google.Apis.Auth.OAuth2;
using SendGrid;

namespace NotificationSystem.Application.UseCases.TestProviderConnection;

public class TestProviderConnectionHandler(
    IProviderConfigurationRepository repository,
    ILogger<TestProviderConnectionHandler> logger)
    : IRequestHandler<TestProviderConnectionQuery, Result<TestProviderConnectionResponse>>
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(12);

    private readonly IProviderConfigurationRepository _repository = repository;
    private readonly ILogger<TestProviderConnectionHandler> _logger = logger;

    public async Task<Result<TestProviderConnectionResponse>> Handle(
        TestProviderConnectionQuery request,
        CancellationToken cancellationToken)
    {
        var provider = await _repository.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider is null)
        {
            return Result.Fail(new NotFoundError("Provider configuration", request.ProviderId));
        }

        var config = ProviderConfigurationSecurityHelper.ParseConfigurationObject(provider.ConfigurationJson);

        try
        {
            return provider.Provider switch
            {
                ProviderType.Smtp => Result.Ok(await TestSmtpAsync(provider.Provider, config, cancellationToken)),
                ProviderType.Twilio => Result.Ok(await TestTwilioAsync(provider.Provider, config, cancellationToken)),
                ProviderType.Firebase => Result.Ok(await TestFirebaseAsync(provider.Provider, config, cancellationToken)),
                ProviderType.SendGrid => Result.Ok(await TestSendGridAsync(provider.Provider, config, cancellationToken)),
                _ => Result.Ok(BuildResponse(false, provider.Provider, "Provider not supported", new Dictionary<string, string>()))
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error while testing provider connection. ProviderId: {ProviderId}", provider.Id);
            return Result.Ok(BuildResponse(
                false,
                provider.Provider,
                "Connection test failed",
                new Dictionary<string, string>
                {
                    ["error"] = "Unexpected validation error"
                }));
        }
    }

    private async Task<TestProviderConnectionResponse> TestSmtpAsync(
        ProviderType provider,
        JsonObject config,
        CancellationToken cancellationToken)
    {
        var host = GetRequiredString(config, "host");
        var port = GetInt(config, "port", 587);
        var username = GetString(config, "username");
        var password = GetString(config, "password");
        var enableSsl = GetBool(config, "enableSsl", true);

        if (string.IsNullOrWhiteSpace(host))
        {
            return BuildResponse(false, provider, "SMTP host is required", new Dictionary<string, string>());
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TestTimeout);

        try
        {
            using var client = new SmtpClient();
            var secureSocketOptions = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            await client.ConnectAsync(host, port, secureSocketOptions, timeoutCts.Token);

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                await client.AuthenticateAsync(username, password, timeoutCts.Token);
            }

            await client.DisconnectAsync(true, timeoutCts.Token);

            return BuildResponse(true, provider, "SMTP connection validated", new Dictionary<string, string>
            {
                ["host"] = host,
                ["port"] = port.ToString(),
                ["authenticated"] = (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)).ToString()
            });
        }
        catch (Exception ex)
        {
            return BuildResponse(false, provider, "SMTP connection failed", new Dictionary<string, string>
            {
                ["host"] = host,
                ["error"] = SanitizeMessage(ex.Message)
            });
        }
    }

    private async Task<TestProviderConnectionResponse> TestTwilioAsync(
        ProviderType provider,
        JsonObject config,
        CancellationToken cancellationToken)
    {
        var accountSid = GetRequiredString(config, "accountSid");
        var authToken = GetRequiredString(config, "authToken");

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
        {
            return BuildResponse(false, provider, "Twilio credentials are required", new Dictionary<string, string>());
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TestTimeout);

        try
        {
            TwilioClient.Init(accountSid, authToken);
            var account = await Twilio.Rest.Api.V2010.AccountResource.FetchAsync(pathSid: accountSid);

            return BuildResponse(true, provider, "Twilio account validated", new Dictionary<string, string>
            {
                ["accountSid"] = accountSid,
                ["status"] = account.Status?.ToString() ?? "unknown"
            });
        }
        catch (ApiException ex)
        {
            return BuildResponse(false, provider, "Twilio credential validation failed", new Dictionary<string, string>
            {
                ["accountSid"] = accountSid,
                ["error"] = SanitizeMessage(ex.Message)
            });
        }
    }

    private async Task<TestProviderConnectionResponse> TestFirebaseAsync(
        ProviderType provider,
        JsonObject config,
        CancellationToken cancellationToken)
    {
        var credentialsJson = GetRequiredString(config, "credentialsJson");

        if (string.IsNullOrWhiteSpace(credentialsJson))
        {
            return BuildResponse(false, provider, "Firebase credentialsJson is required", new Dictionary<string, string>());
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TestTimeout);

        try
        {
            var credential = GoogleCredential.FromJson(credentialsJson)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            await credential.UnderlyingCredential.GetAccessTokenForRequestAsync(
                cancellationToken: timeoutCts.Token);

            return BuildResponse(true, provider, "Firebase credentials validated", new Dictionary<string, string>());
        }
        catch (Exception ex)
        {
            return BuildResponse(false, provider, "Firebase credential validation failed", new Dictionary<string, string>
            {
                ["error"] = SanitizeMessage(ex.Message)
            });
        }
    }

    private async Task<TestProviderConnectionResponse> TestSendGridAsync(
        ProviderType provider,
        JsonObject config,
        CancellationToken cancellationToken)
    {
        var apiKey = GetRequiredString(config, "apiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildResponse(false, provider, "SendGrid apiKey is required", new Dictionary<string, string>());
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TestTimeout);

        try
        {
            var client = new SendGridClient(apiKey);
            var response = await client.RequestAsync(
                method: SendGridClient.Method.GET,
                urlPath: "user/profile",
                cancellationToken: timeoutCts.Token);

            var isSuccess = (int)response.StatusCode is >= 200 and < 300;
            return BuildResponse(
                isSuccess,
                provider,
                isSuccess ? "SendGrid API key validated" : "SendGrid API key validation failed",
                new Dictionary<string, string>
                {
                    ["statusCode"] = ((int)response.StatusCode).ToString()
                });
        }
        catch (Exception ex)
        {
            return BuildResponse(false, provider, "SendGrid connection failed", new Dictionary<string, string>
            {
                ["error"] = SanitizeMessage(ex.Message)
            });
        }
    }

    private static TestProviderConnectionResponse BuildResponse(
        bool success,
        ProviderType provider,
        string message,
        Dictionary<string, string> details)
    {
        return new TestProviderConnectionResponse(success, provider, DateTime.UtcNow, message, details);
    }

    private static string GetRequiredString(JsonObject config, string key)
    {
        var value = GetString(config, key);
        return value ?? string.Empty;
    }

    private static string? GetString(JsonObject config, string key)
    {
        foreach (var entry in config)
        {
            if (!entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (entry.Value is null)
            {
                return null;
            }

            return entry.Value.GetValue<string>();
        }

        return null;
    }

    private static int GetInt(JsonObject config, string key, int fallback)
    {
        var value = GetString(config, key);
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static bool GetBool(JsonObject config, string key, bool fallback)
    {
        var value = GetString(config, key);
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static string SanitizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Unknown error";
        }

        return message.Length > 240 ? message[..240] : message;
    }
}
