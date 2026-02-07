using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Common;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Events;

namespace NotificationSystem.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options, IMediator? mediator = null) : DbContext(options)
{
    private readonly IMediator? _mediator = mediator;

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
    public DbSet<EmailChannel> EmailChannels => Set<EmailChannel>();
    public DbSet<SmsChannel> SmsChannels => Set<SmsChannel>();
    public DbSet<PushChannel> PushChannels => Set<PushChannel>();
    public DbSet<BulkNotificationJob> BulkNotificationJobs => Set<BulkNotificationJob>();
    public DbSet<BulkNotificationItem> BulkNotificationItems => Set<BulkNotificationItem>();

    // Auth entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ProviderConfiguration> ProviderConfigurations => Set<ProviderConfiguration>();

    // Audit Log
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        if (_mediator != null)
        {
            await DispatchDomainEventsAsync(cancellationToken);
        }

        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<Notification>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            // Wrap domain event in a notification wrapper for MediatR
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);

            await _mediator!.Publish(notification!, cancellationToken);
        }
    }
}
