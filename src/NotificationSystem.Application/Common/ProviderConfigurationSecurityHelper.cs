using System.Text.Json;
using System.Text.Json.Nodes;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Common;

public static class ProviderConfigurationSecurityHelper
{
    public static JsonObject ParseConfigurationObject(string configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
            return new JsonObject();

        var parsed = JsonNode.Parse(configurationJson) as JsonObject;
        return parsed ?? new JsonObject();
    }

    public static JsonObject ParseConfigurationObject(JsonElement configuration)
    {
        if (configuration.ValueKind != JsonValueKind.Object)
            return new JsonObject();

        var parsed = JsonNode.Parse(configuration.GetRawText()) as JsonObject;
        return parsed ?? new JsonObject();
    }

    public static JsonObject MergeWithSecretPreservation(
        ProviderType providerType,
        JsonObject existingConfiguration,
        JsonObject incomingConfiguration)
    {
        var merged = (JsonObject?)existingConfiguration.DeepClone() ?? new JsonObject();

        foreach (var entry in incomingConfiguration)
        {
            var existingKey = FindKey(merged, entry.Key);
            if (existingKey is not null && existingKey != entry.Key)
                merged.Remove(existingKey);

            merged[entry.Key] = entry.Value?.DeepClone();
        }

        foreach (var secretField in GetSecretFields(providerType))
        {
            var incomingKey = FindKey(incomingConfiguration, secretField);
            if (incomingKey is null)
                continue;

            var incomingValue = incomingConfiguration[incomingKey];
            if (!IsEmptySecretValue(incomingValue))
                continue;

            var existingKey = FindKey(existingConfiguration, secretField);
            if (existingKey is null)
            {
                merged.Remove(incomingKey);
                continue;
            }

            merged[incomingKey] = existingConfiguration[existingKey]?.DeepClone();
        }

        return merged;
    }

    public static (Dictionary<string, object?> SafeConfiguration, Dictionary<string, bool> SecretConfigured) BuildSafeConfiguration(
        ProviderType providerType,
        JsonObject originalConfiguration)
    {
        var safeConfiguration = (JsonObject?)originalConfiguration.DeepClone() ?? new JsonObject();
        var secretConfigured = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var secretField in GetSecretFields(providerType))
        {
            var key = FindKey(safeConfiguration, secretField);
            var configured = key is not null && IsConfiguredValue(safeConfiguration[key]);
            secretConfigured[$"{secretField}Configured"] = configured;

            if (key is not null)
                safeConfiguration.Remove(key);
        }

        var configurationDictionary = JsonSerializer.Deserialize<Dictionary<string, object?>>(
            safeConfiguration.ToJsonString()) ?? new Dictionary<string, object?>();

        return (configurationDictionary, secretConfigured);
    }

    public static string[] GetSecretFields(ProviderType providerType) => providerType switch
    {
        ProviderType.Smtp => ["password"],
        ProviderType.Twilio => ["authToken"],
        ProviderType.SendGrid => ["apiKey"],
        ProviderType.Firebase => ["credentialsJson"],
        ProviderType.AwsSes => ["accessKeyId", "secretAccessKey", "sessionToken"],
        _ => []
    };

    private static bool IsConfiguredValue(JsonNode? node)
    {
        if (node is null)
            return false;

        if (node is JsonValue value && value.TryGetValue<string>(out var str))
            return !string.IsNullOrWhiteSpace(str);

        return node.ToJsonString().Length > 0;
    }

    private static bool IsEmptySecretValue(JsonNode? node)
    {
        if (node is null)
            return true;

        if (node is JsonValue value && value.TryGetValue<string>(out var str))
            return string.IsNullOrWhiteSpace(str);

        return false;
    }

    private static string? FindKey(JsonObject obj, string expected)
    {
        foreach (var entry in obj)
        {
            if (entry.Key.Equals(expected, StringComparison.OrdinalIgnoreCase))
                return entry.Key;
        }

        return null;
    }
}
