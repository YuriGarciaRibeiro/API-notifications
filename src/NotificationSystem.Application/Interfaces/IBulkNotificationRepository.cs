namespace NotificationSystem.Application.Interfaces;

public interface IBulkNotificationRepository
{
    public Task<IEnumerable<BulkNotificationItem>> GetItemsByJobIdAsync(Guid jobId, NotificationStatus? status = null, CancellationToken cancellationToken = default);
    public Task<BulkNotificationJob?> GetWithItemsAsync(Guid jobId, CancellationToken cancellationToken = default);
    public Task<int> GetProcessedCountAsync(Guid jobId, CancellationToken cancellationToken= default);
    public Task UpdateItemStatusAsync(Guid itemId, NotificationStatus stats, string? ErrorMessage = null, Guid? notificationId = null, CancellationToken cancellationToken = default);
    public Task IncrementProcessedCountAssync(Guid jobId, BulkJobStatus status, CancellationToken cancellationToken = default);
    public Task AddErrorMessageAsync(Guid jobId, string erroMessage, CancellationToken cancellationToken = default);
    public Task AddItemsAsync(Guid jobId, IEnumerable<BulkNotificationItem> items, CancellationToken cancellationToken = default);
    public Task CreateJobAsync(BulkNotificationJob job, CancellationToken cancellationToken = default);
    public Task UpdateJobStatusAsync(Guid jobId, BulkJobStatus status, CancellationToken cancellationToken = default);
    public Task UpdateJobAsync(BulkNotificationJob job, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<BulkNotificationJob> Jobs, int TotalCount)> GetAllAsync(int page, int pageSize, string? status = null, string sortBy = "createdAt", string sortOrder = "desc", CancellationToken cancellationToken = default);
}