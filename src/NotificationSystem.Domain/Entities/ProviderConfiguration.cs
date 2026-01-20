namespace NotificationSystem.Domain.Entities;

public class ProviderConfiguration
{
    public Guid Id { get; set; }
    public ChannelType ChannelType { get; set; }
    public ProviderType Provider { get; set; }
    public string ConfigurationJson { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool isPrimary { get; set; } = false;
    public int Priority { get; set; } = 0;
}

public enum ProviderType
{
    Smtp = 1,
    Twilio = 2,
    Firebase = 3,
}