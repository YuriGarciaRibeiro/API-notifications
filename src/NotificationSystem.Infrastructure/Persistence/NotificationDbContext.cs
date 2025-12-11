using Microsoft.EntityFrameworkCore;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
    public DbSet<EmailChannel> EmailChannels => Set<EmailChannel>();
    public DbSet<SmsChannel> SmsChannels => Set<SmsChannel>();
    public DbSet<PushChannel> PushChannels => Set<PushChannel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
