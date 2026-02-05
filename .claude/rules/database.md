# Entity Framework Core & Database Patterns - Notification System

**Version**: .NET 10.0 + PostgreSQL 16  
**ORM**: Entity Framework Core 10.0.1  
**Pattern**: Code First with Migrations  
**Last Update**: 02/05/2026

---

## 📋 Table of Contents

1. [DbContext Configuration](#dbcontext-configuration)
2. [Entity Mappings](#entity-mappings)
3. [Inheritance Mapping (TPT)](#inheritance-mapping-tpt)
4. [Migrations](#migrations)
5. [Repository Pattern](#repository-pattern)
6. [Query Optimization](#query-optimization)
7. [Indexes & Performance](#indexes--performance)
8. [Connection Strings](#connection-strings)
9. [Seeding](#seeding)
10. [Common Patterns](#common-patterns)

---

## DbContext Configuration

### NotificationContext Setup
```csharp
public class NotificationContext : DbContext
{
    public NotificationContext(DbContextOptions<NotificationContext> options)
        : base(options)
    {
    }

    // DbSets for all aggregates
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<NotificationTemplate> Templates { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Load all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationContext).Assembly);

        // Configure behavior
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Add soft delete query filter
            var softDeleteInterface = typeof(ISoftDelete);
            if (softDeleteInterface.IsAssignableFrom(entity.ClrType))
            {
                var parameter = Expression.Parameter(entity.ClrType);
                var filterExpression = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(ISoftDelete.DeletedAt)),
                        Expression.Constant(null)
                    ),
                    parameter
                );
                entity.QueryFilter = filterExpression;
            }
        }
    }

    // Save changes with domain event handling
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capture domain events before saving
        var entities = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => ((Entity)e.Entity).DomainEvents)
            .ToList();

        // Dispatch domain events (can be customized for your needs)
        foreach (var domainEvent in domainEvents)
        {
            // Events will be published by MediatR behavior
        }

        // Save to database
        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events
        foreach (var entity in entities)
        {
            ((Entity)entity.Entity).ClearDomainEvents();
        }

        return result;
    }
}
```

### Program.cs Configuration
```csharp
builder.Services.AddDbContext<NotificationContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    options
        .UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelaySeconds: 10,
                errorCodesToAdd: null);
            
            npgsqlOptions.CommandTimeout(30);
        })
        .UseSnakeCaseNamingConvention()
        .LogTo(Console.WriteLine, LogLevel.Information); // Remove in production
});
```

---

## Entity Mappings

### Base Entity Configuration
```csharp
public abstract class Entity
{
    [Key]
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } // Soft delete

    private readonly List<DomainEvent> _domainEvents = new();
    
    [NotMapped]
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public interface ISoftDelete
{
    DateTime? DeletedAt { get; }
}
```

### Notification Entity Configuration
```csharp
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Primary key
        builder.HasKey(n => n.Id);

        // Properties
        builder.Property(n => n.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Body)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(n => n.Recipient)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Template)
            .WithMany()
            .HasForeignKey(n => n.TemplateId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => new { n.RecipientId, n.Status });

        // Soft delete
        builder.HasQueryFilter(n => n.DeletedAt == null);

        // Table configuration
        builder.ToTable("notifications");
    }
}

public class Notification : Entity, ISoftDelete
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid RecipientId { get; set; }
    public Guid? TemplateId { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }

    // Navigation properties
    public virtual User Recipient { get; set; } = null!;
    public virtual NotificationTemplate? Template { get; set; }
    public virtual ICollection<NotificationChannel> Channels { get; set; } = new List<NotificationChannel>();
}
```

### User Entity Configuration
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        // Unique constraint
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Relationships
        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.Recipient)
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Subscriptions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete
        builder.HasQueryFilter(u => u.DeletedAt == null);

        builder.ToTable("users");
    }
}

public class User : Entity, ISoftDelete
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
}
```

---

## Inheritance Mapping (TPT)

### Table-Per-Type Pattern

```
Database Tables:
- notification_channels (base table)
- email_notification_channels (inherits from base)
- sms_notification_channels (inherits from base)
- push_notification_channels (inherits from base)
```

### Base Class
```csharp
public abstract class NotificationChannel : Entity
{
    public Guid NotificationId { get; set; }
    public NotificationChannelType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }

    // Navigation
    public virtual Notification Notification { get; set; } = null!;
}

public enum NotificationChannelType
{
    Email = 1,
    Sms = 2,
    Push = 3
}
```

### Derived Classes
```csharp
// Email
public class EmailChannel : NotificationChannel
{
    public string RecipientEmail { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? ReplyTo { get; set; }
}

// SMS
public class SmsChannel : NotificationChannel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}

// Push
public class PushChannel : NotificationChannel
{
    public string DeviceToken { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? CustomData { get; set; }
}
```

### TPT Configuration
```csharp
public class NotificationChannelConfiguration : IEntityTypeConfiguration<NotificationChannel>
{
    public void Configure(EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.HasKey(nc => nc.Id);

        builder.Property(nc => nc.Type)
            .HasConversion<string>();

        builder.Property(nc => nc.Status)
            .HasConversion<string>();

        builder.HasOne(nc => nc.Notification)
            .WithMany(n => n.Channels)
            .HasForeignKey(nc => nc.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Discriminator for TPT
        builder.HasDiscriminator(nc => nc.Type)
            .HasValue<EmailChannel>(NotificationChannelType.Email)
            .HasValue<SmsChannel>(NotificationChannelType.Sms)
            .HasValue<PushChannel>(NotificationChannelType.Push);

        // Indexes
        builder.HasIndex(nc => nc.NotificationId);
        builder.HasIndex(nc => nc.Status);

        builder.ToTable("notification_channels");
    }
}

public class EmailChannelConfiguration : IEntityTypeConfiguration<EmailChannel>
{
    public void Configure(EntityTypeBuilder<EmailChannel> builder)
    {
        builder.Property(ec => ec.RecipientEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(ec => ec.RecipientEmail);

        builder.ToTable("email_notification_channels");
    }
}

public class SmsChannelConfiguration : IEntityTypeConfiguration<SmsChannel>
{
    public void Configure(EntityTypeBuilder<SmsChannel> builder)
    {
        builder.Property(sc => sc.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(sc => sc.PhoneNumber);

        builder.ToTable("sms_notification_channels");
    }
}

public class PushChannelConfiguration : IEntityTypeConfiguration<PushChannel>
{
    public void Configure(EntityTypeBuilder<PushChannel> builder)
    {
        builder.Property(pc => pc.DeviceToken)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pc => pc.CustomData)
            .HasColumnType("jsonb");

        builder.HasIndex(pc => pc.DeviceToken);

        builder.ToTable("push_notification_channels");
    }
}
```

---

## Migrations

### Creating Migrations
```bash
# Add migration
dotnet ef migrations add CreateNotificationSchema \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api \
    --context NotificationContext

# List migrations
dotnet ef migrations list \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api

# Remove last migration (not applied yet)
dotnet ef migrations remove \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api
```

### Applying Migrations
```bash
# Update database to latest migration
dotnet ef database update \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api

# Update to specific migration
dotnet ef database update CreateNotificationSchema \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api

# Rollback to previous migration
dotnet ef database update 0 \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api
```

### Generated Migration Example
```csharp
public partial class CreateNotificationSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                email = table.Column<string>(maxLength: 255, nullable: false),
                password_hash = table.Column<string>(maxLength: 255, nullable: false),
                first_name = table.Column<string>(maxLength: 100, nullable: true),
                last_name = table.Column<string>(maxLength: 100, nullable: true),
                is_active = table.Column<bool>(nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(nullable: true),
                deleted_at = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateTable(
            name: "notifications",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                subject = table.Column<string>(maxLength: 200, nullable: false),
                body = table.Column<string>(type: "text", nullable: false),
                recipient_id = table.Column<Guid>(nullable: false),
                status = table.Column<string>(maxLength: 50, nullable: false),
                created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(nullable: true),
                deleted_at = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_notifications", x => x.id);
                table.ForeignKey(
                    name: "fk_notifications_users_recipient_id",
                    column: x => x.recipient_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "notifications");
        migrationBuilder.DropTable(name: "users");
    }
}
```

---

## Repository Pattern

### Generic Repository
```csharp
public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected readonly NotificationContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(NotificationContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Specialized Repository
```csharp
public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(NotificationContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.RecipientId == userId)
            .Include(n => n.Channels)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.Status == NotificationStatus.Pending)
            .Include(n => n.Channels)
            .AsNoTracking()
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(n => n.RecipientId == userId, cancellationToken);
    }
}
```

---

## Query Optimization

### Eager Loading (Include)
```csharp
// ✅ CORRECT - eager loading prevents N+1
var notifications = await _context.Notifications
    .Include(n => n.Recipient)
    .Include(n => n.Channels)
    .Include(n => n.Template)
    .AsNoTracking()
    .Where(n => n.Status == NotificationStatus.Pending)
    .ToListAsync();

// Multiple levels
var notifications = await _context.Notifications
    .Include(n => n.Recipient)
        .ThenInclude(u => u.Subscriptions)
    .Include(n => n.Channels)
    .AsNoTracking()
    .ToListAsync();
```

### Projection (Select)
```csharp
// ✅ CORRECT - only load needed columns
var notificationDtos = await _context.Notifications
    .AsNoTracking()
    .Where(n => n.RecipientId == userId)
    .Select(n => new NotificationDto
    {
        Id = n.Id,
        Subject = n.Subject,
        Status = n.Status,
        CreatedAt = n.CreatedAt,
        RecipientEmail = n.Recipient.Email
    })
    .ToListAsync();

// ❌ WRONG - loads entire entity then filters
var all = await _context.Notifications.ToListAsync();
var filtered = all.Select(n => new NotificationDto { ... }).ToList();
```

### AsNoTracking() for Read-Only
```csharp
// ✅ CORRECT - doesn't track changes
var notifications = await _context.Notifications
    .AsNoTracking()
    .ToListAsync();

// ❌ WRONG - tracks unnecessarily
var notifications = await _context.Notifications
    .Include(n => n.Channels)
    .ToListAsync();
```

### Skip/Take for Pagination
```csharp
// ✅ CORRECT - database-level pagination
var page = 1;
var pageSize = 20;
var notifications = await _context.Notifications
    .AsNoTracking()
    .OrderByDescending(n => n.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// ❌ WRONG - load all then paginate in memory
var all = await _context.Notifications.ToListAsync();
var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
```

---

## Indexes & Performance

### Index Definitions
```csharp
// Single column
builder.HasIndex(n => n.RecipientId);

// Composite index
builder.HasIndex(n => new { n.RecipientId, n.Status })
    .HasName("idx_notifications_recipient_status");

// Unique index
builder.HasIndex(u => u.Email)
    .IsUnique();

// Filtered index (included columns)
builder.HasIndex(n => n.Status)
    .HasFilter("deleted_at IS NULL")
    .IsUnique(false);
```

### Index Strategy
```csharp
public class NotificationIndexStrategy
{
    // Write-heavy tables: minimal indexes
    // Read-heavy tables: more indexes
    
    // Example for Notification:
    // 1. PK (Id)
    // 2. FK (RecipientId) - joins
    // 3. Status - filtering
    // 4. CreatedAt - sorting
    // 5. Composite (RecipientId, Status) - common filters
}
```

---

## Connection Strings

### appsettings.json (Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=NotificationSystem;Username=admin;Password=password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=50;"
  }
}
```

### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db.example.com;Port=5432;Database=NotificationSystem;Username=dbuser;Password=${DB_PASSWORD};Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;SSL Mode=Require;"
  }
}
```

### Environment Variables
```bash
# Docker/Environment
DB_CONNECTION_STRING="Host=postgres;Port=5432;Database=NotificationSystem;Username=admin;Password=password"
DB_HOST=postgres
DB_PORT=5432
DB_USER=admin
DB_PASSWORD=password
DB_NAME=NotificationSystem
```

---

## Seeding

### Seed Data
```csharp
public static class NotificationContextSeed
{
    public static async Task SeedAsync(this NotificationContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var templates = new List<NotificationTemplate>
        {
            new NotificationTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Welcome",
                Subject = "Welcome to our platform",
                Body = "Hello {{FirstName}}, welcome!"
            }
        };

        await context.Templates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
    }
}

// In Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationContext>();
    await context.Database.MigrateAsync();
    await context.SeedAsync();
}
```

---

## Common Patterns

### Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    INotificationRepository Notifications { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly NotificationContext _context;
    
    public INotificationRepository Notifications { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(NotificationContext context)
    {
        _context = context;
        Notifications = new NotificationRepository(context);
        Users = new UserRepository(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<T> ExecuteTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation();
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Soft Delete Pattern
```csharp
public interface ISoftDelete
{
    DateTime? DeletedAt { get; }
}

public abstract class Entity
{
    public DateTime? DeletedAt { get; set; }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasQueryFilter(n => n.DeletedAt == null);
    }
}

// Soft delete usage
public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
{
    var notification = await GetByIdAsync(id, cancellationToken);
    if (notification == null)
        throw new NotificationNotFoundException(id);

    notification.DeletedAt = DateTime.UtcNow;
    await UpdateAsync(notification, cancellationToken);
}
```

### Auditing Pattern
```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

        var context = eventData.Context;
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

// Register in DI
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddDbContext<NotificationContext>((sp, options) =>
{
    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});
```

---

## Summary Checklist

- ✅ Configure DbContext with proper options
- ✅ Use IEntityTypeConfiguration for entity mappings
- ✅ Implement TPT inheritance correctly
- ✅ Create and apply migrations safely
- ✅ Use Repository pattern for data access
- ✅ Optimize queries: eager loading, projection, AsNoTracking()
- ✅ Index frequently queried columns
- ✅ Connection pooling in production
- ✅ Seed initial data
- ✅ Implement soft deletes
- ✅ Use transactions for critical operations
- ✅ Monitor query performance
