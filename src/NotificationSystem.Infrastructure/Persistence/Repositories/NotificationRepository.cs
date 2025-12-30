using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications
            .Include(n => n.Channels)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount)
    {
        return await _context.Notifications
            .Include(n => n.Channels)
            .Where(n => n.Channels.Any(c => c.Status == NotificationStatus.Pending))
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .Include(n => n.Channels)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateNotificationChannelStatusAsync<TChannel>(Guid notificationId, Guid channelId, NotificationStatus status, string? errorMessage = null) where TChannel : NotificationChannel
    {
        var channel = _context.Set<TChannel>().FirstOrDefault(c => c.Id == channelId && c.NotificationId == notificationId);
        if (channel != null)
        {
            channel.Status = status;
            channel.ErrorMessage = errorMessage;
            _context.Set<TChannel>().Update(channel);
            return _context.SaveChangesAsync();
        }
        return Task.CompletedTask;
    }
}