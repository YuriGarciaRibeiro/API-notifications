using Microsoft.AspNetCore.SignalR;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Api.Hubs;

public class BulkProgressBroadcasterService(
    IServiceScopeFactory scopeFactory,
    IHubContext<BulkProgressHub> hubContext,
    IBulkProgressSubscriptionStore subscriptionStore,
    ILogger<BulkProgressBroadcasterService> logger) : BackgroundService
{
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IHubContext<BulkProgressHub> _hubContext = hubContext;
    private readonly IBulkProgressSubscriptionStore _subscriptionStore = subscriptionStore;
    private readonly ILogger<BulkProgressBroadcasterService> _logger = logger;
    private readonly Dictionary<Guid, string> _lastPayloadHashes = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bulk progress broadcaster started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var trackedJobIds = _subscriptionStore.GetTrackedJobIds();
                if (trackedJobIds.Count > 0)
                {
                    await BroadcastUpdatesAsync(trackedJobIds, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while broadcasting bulk progress updates");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("Bulk progress broadcaster stopped");
    }

    private async Task BroadcastUpdatesAsync(IReadOnlyCollection<Guid> trackedJobIds, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBulkNotificationRepository>();

        foreach (var jobId in trackedJobIds)
        {
            if (!_subscriptionStore.HasSubscribers(jobId))
            {
                _lastPayloadHashes.Remove(jobId);
                continue;
            }

            var job = await repository.GetByIdAsync(jobId, cancellationToken);
            if (job is null)
            {
                continue;
            }

            var payload = BuildPayload(job);
            var payloadHash = BuildPayloadHash(payload);

            if (_lastPayloadHashes.TryGetValue(jobId, out var lastPayloadHash) && lastPayloadHash == payloadHash)
            {
                continue;
            }

            _lastPayloadHashes[jobId] = payloadHash;

            await _hubContext.Clients
                .Group(BulkProgressHub.BuildGroupName(jobId))
                .SendAsync("BulkProgressUpdated", payload, cancellationToken);

            if (IsFinalState(job.Status))
            {
                _lastPayloadHashes.Remove(jobId);
            }
        }
    }

    private static BulkProgressUpdate BuildPayload(BulkNotificationJob job)
    {
        var percent = job.TotalCount > 0
            ? Math.Round(job.ProcessedCount / (double)job.TotalCount * 100, 2)
            : 0;

        return new BulkProgressUpdate(
            job.Id,
            job.Status.ToString(),
            percent,
            job.TotalCount,
            job.ProcessedCount,
            job.SuccessCount,
            job.FailedCount,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.UpdatedAt,
            DateTime.UtcNow);
    }

    private static bool IsFinalState(BulkJobStatus status)
    {
        return status is BulkJobStatus.Completed or BulkJobStatus.Failed or BulkJobStatus.Cancelled;
    }

    private static string BuildPayloadHash(BulkProgressUpdate payload)
    {
        return string.Join('|',
            payload.Status,
            payload.Percent,
            payload.Total,
            payload.Processed,
            payload.Success,
            payload.Failure,
            payload.StartedAt,
            payload.CompletedAt,
            payload.UpdatedAt);
    }
}
