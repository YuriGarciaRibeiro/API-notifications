namespace NotificationSystem.Apllication.Exceptions;

/// <summary>
/// Exception thrown when a provider type is not compatible with a specific factory.
/// </summary>
public class InvalidProviderTypeException : InvalidOperationException
{
    public ProviderType ProviderType { get; }
    public ChannelType ExpectedChannelType { get; }

    public InvalidProviderTypeException(
        ProviderType providerType,
        ChannelType expectedChannelType)
        : base($"Provider type '{providerType}' is not valid for {expectedChannelType} factory. " +
               $"This provider type should be used with a different channel factory.")
    {
        ProviderType = providerType;
        ExpectedChannelType = expectedChannelType;
    }

    public InvalidProviderTypeException(
        ProviderType providerType,
        ChannelType expectedChannelType,
        string customMessage)
        : base(customMessage)
    {
        ProviderType = providerType;
        ExpectedChannelType = expectedChannelType;
    }

    public InvalidProviderTypeException(
        ProviderType providerType,
        ChannelType expectedChannelType,
        string customMessage,
        Exception innerException)
        : base(customMessage, innerException)
    {
        ProviderType = providerType;
        ExpectedChannelType = expectedChannelType;
    }
}
