namespace NotificationSystem.Api.Hubs;

public interface IBulkProgressSubscriptionStore
{
    void TrackSubscription(string connectionId, Guid jobId);
    void RemoveSubscription(string connectionId, Guid jobId);
    IReadOnlyCollection<Guid> RemoveConnection(string connectionId);
    IReadOnlyCollection<Guid> GetTrackedJobIds();
    bool HasSubscribers(Guid jobId);
}
