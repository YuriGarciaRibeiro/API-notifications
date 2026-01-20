using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Repositories;

public class NotificationRepository(NotificationDbContext context) : INotificationRepository
{
    private readonly NotificationDbContext _context = context;

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
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
    {
        return await _context.Notifications.CountAsync(cancellationToken);
    }

    public async Task UpdateNotificationChannelStatusAsync<TChannel>(Guid notificationId, Guid channelId, NotificationStatus status, string? errorMessage = null) where TChannel : NotificationChannel
    {
        var channel = await _context.Set<TChannel>()
            .FirstOrDefaultAsync(c => c.Id == channelId && c.NotificationId == notificationId);

        if (channel != null)
        {
            channel.Status = status;
            channel.ErrorMessage = errorMessage;
            _context.Set<TChannel>().Update(channel);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<NotificationStats> GetStatsAsync(CancellationToken cancellationToken)
    {
        var notifications = await _context.Notifications
            .Include(n => n.Channels)
            .ToListAsync(cancellationToken);

        var allChannels = notifications.SelectMany(n => n.Channels).ToList();

        var total = allChannels.Count;
        var sent = allChannels.Count(c => c.Status == NotificationStatus.Sent);
        var pending = allChannels.Count(c => c.Status == NotificationStatus.Pending);
        var failed = allChannels.Count(c => c.Status == NotificationStatus.Failed);

        var emailCount = allChannels.Count(c => c.Type == ChannelType.Email);
        var smsCount = allChannels.Count(c => c.Type == ChannelType.Sms);
        var pushCount = allChannels.Count(c => c.Type == ChannelType.Push);

        return new NotificationStats(
            total,
            sent,
            pending,
            failed,
            emailCount,
            smsCount,
            pushCount
        );
    }

    public async Task<NotificationStats> GetStatsForPeriodAsync(DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        var notifications = await _context.Notifications
            .Include(n => n.Channels)
            .Where(n => n.CreatedAt >= start && n.CreatedAt < end)
            .ToListAsync(cancellationToken);

        var allChannels = notifications.SelectMany(n => n.Channels).ToList();

        var total = allChannels.Count;
        var sent = allChannels.Count(c => c.Status == NotificationStatus.Sent);
        var pending = allChannels.Count(c => c.Status == NotificationStatus.Pending);
        var failed = allChannels.Count(c => c.Status == NotificationStatus.Failed);

        var emailCount = allChannels.Count(c => c.Type == ChannelType.Email);
        var smsCount = allChannels.Count(c => c.Type == ChannelType.Sms);
        var pushCount = allChannels.Count(c => c.Type == ChannelType.Push);

        return new NotificationStats(
            total,
            sent,
            pending,
            failed,
            emailCount,
            smsCount,
            pushCount
        );
    }
}