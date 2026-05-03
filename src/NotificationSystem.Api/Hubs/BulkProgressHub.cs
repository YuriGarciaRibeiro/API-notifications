using Microsoft.AspNetCore.SignalR;

namespace NotificationSystem.Api.Hubs;

public class BulkProgressHub(IBulkProgressSubscriptionStore subscriptionStore) : Hub
{
    private readonly IBulkProgressSubscriptionStore _subscriptionStore = subscriptionStore;

    public async Task Subscribe(string jobId)
    {
        if (!Guid.TryParse(jobId, out var parsedJobId))
        {
            throw new HubException("Invalid jobId");
        }

        var group = BuildGroupName(parsedJobId);
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        _subscriptionStore.TrackSubscription(Context.ConnectionId, parsedJobId);
    }

    public async Task Unsubscribe(string jobId)
    {
        if (!Guid.TryParse(jobId, out var parsedJobId))
        {
            throw new HubException("Invalid jobId");
        }

        var group = BuildGroupName(parsedJobId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        _subscriptionStore.RemoveSubscription(Context.ConnectionId, parsedJobId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _subscriptionStore.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public static string BuildGroupName(Guid jobId) => $"bulk-job:{jobId}";
}
