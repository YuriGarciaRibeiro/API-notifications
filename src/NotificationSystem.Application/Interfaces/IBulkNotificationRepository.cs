namespace NotificationSystem.Application.Interfaces;

public interface IBulkNotificationRepository
{
    Task<IEnumerable<BulkNotificationItem>> GetItemsByJobIdAsync(Guid jobId, NotificationStatus? status = null, CancellationToken cancellationToken = default);
    Task<BulkNotificationJob?> GetWithItemsAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<int> GetProcessedCountAsync(Guid jobId, CancellationToken cancellationToken= default);
    Task UpdateItemStatusAsync(Guid itemId, NotificationStatus stats, string? ErrorMessage = null, Guid? notificationId = null, CancellationToken cancellationToken = default);
    Task IncrementProcessedCountAssync(Guid jobId, BulkJobStatus status, CancellationToken cancellationToken = default);
    Task AddErrorMessageAsync(Guid jobId, string erroMessage, CancellationToken cancellationToken = default);
    Task AddItemsAsync(Guid jobId, IEnumerable<BulkNotificationItem> items, CancellationToken cancellationToken = default);
    Task CreateJobAsync(BulkNotificationJob job, CancellationToken cancellationToken = default);
    Task UpdateJobStatusAsync(Guid jobId, BulkJobStatus status, CancellationToken cancellationToken = default);
    Task UpdateJobAsync(BulkNotificationJob job, CancellationToken cancellationToken = default);
    Task<(IEnumerable<BulkNotificationJob> Jobs, int TotalCount)> GetAllAsync(int page, int pageSize, string? status = null, string sortBy = "createdAt", string sortOrder = "desc", CancellationToken cancellationToken = default);
}