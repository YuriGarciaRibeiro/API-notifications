using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Repositories;

public class BulkNotificationRepository(NotificationDbContext context) : IBulkNotificationRepository
{
    private readonly NotificationDbContext _context = context;

    public async Task AddErrorMessageAsync(Guid jobId, string erroMessage, CancellationToken cancellationToken = default)
    {
        var job = await _context.bulkNotificationJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
        if (job != null) job.ErrorMessages.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {erroMessage}");
        _context.bulkNotificationJobs.Update(job!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddItemsAsync(Guid jobId, IEnumerable<BulkNotificationItem> items, CancellationToken cancellationToken = default)
    {
        var job = await _context.bulkNotificationJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
        if (job != null)
        {
            foreach (var item in items)
            {
                item.BulkJobId = jobId;
                _context.BulkNotificationItems.Add(item);
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task CreateJobAsync(BulkNotificationJob job, CancellationToken cancellationToken = default)
    {
        _context.bulkNotificationJobs.Add(job);
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<BulkNotificationItem>> GetItemsByJobIdAsync(Guid jobId, NotificationStatus? status, CancellationToken cancellationToken = default)
    {
        var items = _context.BulkNotificationItems
            .Where(i => i.BulkJobId == jobId && (status == null || i.Status == status));

        return await items.ToListAsync(cancellationToken);
    }

    public async Task<int> GetProcessedCountAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _context.bulkNotificationJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
        return job?.ProcessedCount ?? 0;
    }

    public async Task<BulkNotificationJob?> GetWithItemsAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = _context.bulkNotificationJobs
            .Where(j => j.Id == jobId)
            .Include(j => j.Items)
            .FirstOrDefaultAsync(cancellationToken);

        return await job;
    }

    public async Task IncrementProcessedCountAssync(Guid jobId, BulkJobStatus status, CancellationToken cancellationToken = default)
    {
        var job = await _context.bulkNotificationJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
        if (job != null)
        {
            job.ProcessedCount++;
            job.UpdatedAt = DateTime.UtcNow;
            _context.bulkNotificationJobs.Update(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateItemStatusAsync(Guid itemId, NotificationStatus stats, string? ErrorMessage = null, Guid? notificationId = null, CancellationToken cancellationToken = default)
    {
        var item = await _context.BulkNotificationItems.FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item != null)
        {
            item.Status = stats;
            item.ErrorMessage = ErrorMessage;
            item.UpdatedAt = DateTime.UtcNow;

            if (stats == NotificationStatus.Sent) item.SentAt = DateTime.UtcNow;
            if (notificationId.HasValue) item.NotificationId = notificationId;
            _context.BulkNotificationItems.Update(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateJobStatusAsync(Guid jobId, BulkJobStatus status, CancellationToken cancellationToken = default)
    {
        var job = await _context.bulkNotificationJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
        if (job != null)
        {
            job.Status = status;
            job.UpdatedAt = DateTime.UtcNow;

            if (status == BulkJobStatus.Processing && !job.StartedAt.HasValue)
                job.StartedAt = DateTime.UtcNow;

            if (status == BulkJobStatus.Completed && !job.CompletedAt.HasValue)
                job.CompletedAt = DateTime.UtcNow;

            _context.bulkNotificationJobs.Update(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateJobAsync(BulkNotificationJob job, CancellationToken cancellationToken = default)
    {
        _context.bulkNotificationJobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<BulkNotificationJob> Jobs, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? status = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        CancellationToken cancellationToken = default)
    {
        var query = _context.bulkNotificationJobs.AsQueryable();

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<BulkJobStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                query = query.Where(j => j.Status == parsedStatus);
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Sort
        query = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase)
            ? sortBy.ToLower() switch
            {
                "name" => query.OrderBy(j => j.Name),
                "status" => query.OrderBy(j => j.Status),
                "createdat" => query.OrderBy(j => j.CreatedAt),
                _ => query.OrderBy(j => j.CreatedAt)
            }
            : sortBy.ToLower() switch
            {
                "name" => query.OrderByDescending(j => j.Name),
                "status" => query.OrderByDescending(j => j.Status),
                "createdat" => query.OrderByDescending(j => j.CreatedAt),
                _ => query.OrderByDescending(j => j.CreatedAt)
            };

        // Pagination
        var jobs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (jobs, totalCount);
    }
}