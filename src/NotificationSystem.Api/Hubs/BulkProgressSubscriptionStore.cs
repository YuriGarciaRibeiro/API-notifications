using System.Collections.Concurrent;

namespace NotificationSystem.Api.Hubs;

public class BulkProgressSubscriptionStore : IBulkProgressSubscriptionStore
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, byte>> _connectionJobs = new();
    private readonly ConcurrentDictionary<Guid, int> _jobSubscribers = new();

    public void TrackSubscription(string connectionId, Guid jobId)
    {
        var jobs = _connectionJobs.GetOrAdd(connectionId, _ => new ConcurrentDictionary<Guid, byte>());

        if (jobs.TryAdd(jobId, 0))
        {
            _jobSubscribers.AddOrUpdate(jobId, 1, (_, count) => count + 1);
        }
    }

    public void RemoveSubscription(string connectionId, Guid jobId)
    {
        if (_connectionJobs.TryGetValue(connectionId, out var jobs) && jobs.TryRemove(jobId, out _))
        {
            DecrementSubscriberCount(jobId);

            if (jobs.IsEmpty)
            {
                _connectionJobs.TryRemove(connectionId, out _);
            }
        }
    }

    public IReadOnlyCollection<Guid> RemoveConnection(string connectionId)
    {
        if (!_connectionJobs.TryRemove(connectionId, out var jobs))
        {
            return [];
        }

        var removedJobIds = new List<Guid>(jobs.Keys);
        foreach (var jobId in removedJobIds)
        {
            DecrementSubscriberCount(jobId);
        }

        return removedJobIds;
    }

    public IReadOnlyCollection<Guid> GetTrackedJobIds()
    {
        return _jobSubscribers.Keys.ToList();
    }

    public bool HasSubscribers(Guid jobId)
    {
        return _jobSubscribers.TryGetValue(jobId, out var count) && count > 0;
    }

    private void DecrementSubscriberCount(Guid jobId)
    {
        while (true)
        {
            if (!_jobSubscribers.TryGetValue(jobId, out var currentCount))
            {
                return;
            }

            if (currentCount <= 1)
            {
                if (_jobSubscribers.TryRemove(jobId, out _))
                {
                    return;
                }

                continue;
            }

            if (_jobSubscribers.TryUpdate(jobId, currentCount - 1, currentCount))
            {
                return;
            }
        }
    }
}
