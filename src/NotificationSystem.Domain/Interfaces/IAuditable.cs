namespace NotificationSystem.Domain.Interfaces;

/// <summary>
/// Marker interface to indicate that an entity should be tracked in the audit log.
/// Only entities implementing this interface will have their changes recorded.
/// </summary>
public interface IAuditable
{
}
