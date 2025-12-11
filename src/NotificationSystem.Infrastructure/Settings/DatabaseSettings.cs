namespace NotificationSystem.Infrastructure.Settings;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;
}
