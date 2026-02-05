# C# Coding Guidelines - Notification System API

**Version**: .NET 10.0  
**Project**: NotificationSystem  
**Last Update**: 02/05/2026

---

## 📋 Table of Contents

1. [Naming Conventions](#naming-conventions)
2. [Nullable Reference Types](#nullable-reference-types)
3. [Async/Await Rules](#asyncawait-rules)
4. [Dependency Injection](#dependency-injection)
5. [Error Handling](#error-handling)
6. [LINQ Usage](#linq-usage)
7. [Logging Best Practices](#logging-best-practices)
8. [Performance Tips](#performance-tips)
9. [Code Organization](#code-organization)

---

## Naming Conventions

### Classes, Methods, Properties
```csharp
// ✅ CORRECT
public class NotificationService { }
public IEnumerable<Notification> GetNotifications() { }
public string Subject { get; set; }
public const string DefaultTemplate = "default";

// ❌ WRONG
public class notification_service { }
public IEnumerable<Notification> get_notifications() { }
public string subject { get; set; }
public string defaultTemplate = "default";
```

### Local Variables & Parameters
```csharp
// ✅ CORRECT
var notificationId = notification.Id;
void SendNotification(string recipientEmail, bool isUrgent) { }

// ❌ WRONG
var NotificationId = notification.Id;
void SendNotification(string RecipientEmail, bool IsUrgent) { }
```

### Interfaces
```csharp
// ✅ CORRECT
public interface INotificationRepository { }
public interface IEmailSender { }
public interface IChannelStrategy { }

// ❌ WRONG
public interface NotificationRepository { }
public interface EmailSender { }
```

### Private Fields
```csharp
public class NotificationService
{
    // ✅ CORRECT - underscore prefix for private fields
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationService> _logger;

    // ❌ WRONG
    private readonly INotificationRepository repository;
    private readonly ILogger<NotificationService> logger;
}
```

### Constants & Enums
```csharp
// ✅ CORRECT
public const string MaxRetries = "MAX_RETRIES";
public enum NotificationStatus { Pending, Sent, Failed }
public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

// ❌ WRONG
public const string max_retries = "max_retries";
public enum notificationstatus { pending, sent, failed }
```

### Boolean Variables
```csharp
// ✅ CORRECT
public bool IsActive { get; set; }
public bool HasErrors { get; set; }
public bool ShouldRetry { get; set; }
bool isValidEmail = email.Contains("@");

// ❌ WRONG
public bool Active { get; set; }
public bool Errors { get; set; }
public bool RetryEnabled { get; set; }
```

---

## Nullable Reference Types

**Project Setting**: `<Nullable>enable</Nullable>` (enabled in .csproj)

### Non-Nullable References
```csharp
// ✅ CORRECT - required property
public class NotificationCreateDto
{
    public string Subject { get; set; } = null!; // Non-null required
    public string Body { get; set; } = null!;
}

// ✅ CORRECT - constructor ensures initialization
public class Notification
{
    public string Subject { get; }
    public string Body { get; }

    public Notification(string subject, string body)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}
```

### Nullable References
```csharp
// ✅ CORRECT - nullable property
public class NotificationDto
{
    public string Subject { get; set; } = null!;
    public string? Description { get; set; } // Can be null
    public string? ReplyTo { get; set; }
}

// ✅ CORRECT - null-coalescing
public string GetDisplayName()
{
    return Name ?? "Unknown";
}
```

### Handling Nullability
```csharp
// ✅ CORRECT - null check
if (user?.Email != null)
{
    await SendNotification(user.Email);
}

// ✅ CORRECT - null-coalescing operator
var email = user?.Email ?? "noreply@example.com";

// ✅ CORRECT - null-conditional
var count = notifications?.Count ?? 0;
```

---

## Async/Await Rules

### Always Use Async for I/O Operations
```csharp
// ✅ CORRECT
public async Task<Notification> GetNotificationAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);
}

public async Task SendEmailAsync(string to, string subject, string body)
{
    await _emailService.SendAsync(to, subject, body);
}

// ❌ WRONG - blocking call
public Notification GetNotification(Guid id)
{
    return _repository.GetById(id).Result; // DEADLOCK RISK!
}
```

### ConfigureAwait in Libraries
```csharp
// ✅ CORRECT - library code
public async Task<Notification> GetNotificationAsync(Guid id)
{
    return await _repository.GetByIdAsync(id).ConfigureAwait(false);
}

// ✅ ACCEPTABLE - ASP.NET Core app (sync context less critical)
public async Task SendNotificationAsync(Notification notification)
{
    await _service.SendAsync(notification);
}
```

### Async Event Handlers
```csharp
// ✅ CORRECT
public class NotificationCreatedEventHandler
{
    public async Task Handle(NotificationCreated @event, CancellationToken cancellationToken)
    {
        await _repository.SaveAsync(@event.Notification, cancellationToken);
    }
}

// ❌ WRONG - .Result blocks thread
public void OnNotificationCreated(Notification notification)
{
    _service.SendAsync(notification).Result;
}
```

### Avoid Async Void (Except Event Handlers)
```csharp
// ✅ CORRECT
public async Task ProcessNotificationsAsync()
{
    var notifications = await _repository.GetPendingAsync();
    foreach (var notification in notifications)
    {
        await ProcessNotificationAsync(notification);
    }
}

// ❌ WRONG - async void can't be awaited
public async void ProcessNotificationsAsync()
{
    var notifications = await _repository.GetPendingAsync();
    // Exception might be unobserved!
}
```

---

## Dependency Injection

### Constructor Injection
```csharp
// ✅ CORRECT
public class CreateNotificationCommandHandler
{
    private readonly INotificationRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateNotificationCommandHandler> _logger;

    public CreateNotificationCommandHandler(
        INotificationRepository repository,
        IEmailService emailService,
        ILogger<CreateNotificationCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}

// ❌ WRONG - Service Locator antipattern
public class NotificationService
{
    private readonly IServiceProvider _serviceProvider;

    public void Send(Notification notification)
    {
        var emailService = _serviceProvider.GetService<IEmailService>();
        emailService.Send(notification);
    }
}
```

### Registration Pattern
```csharp
// ✅ CORRECT - in DependencyInjection.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    services.AddScoped<INotificationRepository, NotificationRepository>();
    services.AddScoped<INotificationCommandHandler, CreateNotificationCommandHandler>();
    services.AddScoped<IEmailService, SmtpEmailService>();
    
    services.Configure<EmailSettings>(configuration.GetSection("Email"));
    
    return services;
}
```

### Options Pattern (Configuration)
```csharp
// ✅ CORRECT
public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port);
        // Send email
    }
}
```

---

## Error Handling

### Use Custom Exceptions
```csharp
// ✅ CORRECT
public class NotificationException : Exception
{
    public NotificationException(string message) : base(message) { }
    public NotificationException(string message, Exception inner) : base(message, inner) { }
}

public class NotificationNotFoundException : NotificationException
{
    public Guid NotificationId { get; }

    public NotificationNotFoundException(Guid notificationId)
        : base($"Notification {notificationId} not found")
    {
        NotificationId = notificationId;
    }
}

// ❌ WRONG - too generic
throw new Exception("Notification not found");
```

### Validation Errors
```csharp
// ✅ CORRECT - FluentValidation
public class CreateNotificationValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required");

        RuleFor(x => x.RecipientId)
            .NotEmpty().WithMessage("Recipient is required");
    }
}
```

### Try-Catch Best Practices
```csharp
// ✅ CORRECT - log and re-throw
public async Task<Notification> GetNotificationAsync(Guid id)
{
    try
    {
        return await _repository.GetByIdAsync(id);
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Database error retrieving notification {NotificationId}", id);
        throw new NotificationException("Failed to retrieve notification", ex);
    }
}

// ❌ WRONG - swallowing exception
try
{
    return await _repository.GetByIdAsync(id);
}
catch { }

// ❌ WRONG - generic catch
try
{
    return await _repository.GetByIdAsync(id);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return null;
}
```

---

## LINQ Usage

### Query Syntax vs Method Syntax
```csharp
// ❌ Wrong Query syntax
var activeNotifications = from n in _context.Notifications
                          where n.Status == NotificationStatus.Pending
                          select n;

// ✅ CORRECT Method syntax
var activeNotifications = _context.Notifications
    .Where(n => n.Status == NotificationStatus.Pending)
    .AsNoTracking()
    .ToList();
```

### Deferred Execution
```csharp
// ✅ CORRECT - deferred, won't execute until enumerated
IQueryable<Notification> query = _repository.GetNotifications()
    .Where(n => n.CreatedAt > DateTime.Now.AddDays(-7));

// Execute when needed
var notifications = await query.ToListAsync();

// ❌ WRONG - executing too early
List<Notification> allNotifications = _repository.GetNotifications().ToList();
var recentNotifications = allNotifications
    .Where(n => n.CreatedAt > DateTime.Now.AddDays(-7))
    .ToList(); // Double enumeration
```

### Use AsNoTracking for Read-Only
```csharp
// ✅ CORRECT
public async Task<IEnumerable<NotificationDto>> GetAllAsync()
{
    return await _context.Notifications
        .AsNoTracking()
        .Select(n => new NotificationDto
        {
            Id = n.Id,
            Subject = n.Subject,
            Status = n.Status
        })
        .ToListAsync();
}

// ❌ WRONG - tracking unused
public async Task<IEnumerable<NotificationDto>> GetAllAsync()
{
    return await _context.Notifications
        .Select(n => new NotificationDto { ... })
        .ToListAsync();
}
```

### Avoid N+1 Queries
```csharp
// ✅ CORRECT - eager loading
var notifications = await _context.Notifications
    .Include(n => n.Channels)
    .Include(n => n.User)
    .Where(n => n.Status == NotificationStatus.Pending)
    .AsNoTracking()
    .ToListAsync();

// ❌ WRONG - N+1 query problem
var notifications = await _context.Notifications
    .Where(n => n.Status == NotificationStatus.Pending)
    .ToListAsync();

foreach (var notification in notifications)
{
    var channels = await _context.NotificationChannels // EXECUTED FOR EACH NOTIFICATION
        .Where(c => c.NotificationId == notification.Id)
        .ToListAsync();
}
```

---

## Logging Best Practices

### Use Structured Logging
```csharp
// ✅ CORRECT - structured logging with context
_logger.LogInformation(
    "Creating notification for user {UserId} with subject {Subject}",
    userId,
    subject
);

_logger.LogError(ex,
    "Failed to send notification {NotificationId} to {RecipientEmail}",
    notificationId,
    recipientEmail
);

// ❌ WRONG - string concatenation
_logger.LogInformation("Creating notification for user " + userId);
_logger.LogError($"Failed to send notification {notificationId}");
```

### Log Levels
```csharp
// ✅ CORRECT log level usage

// Critical: System crashes, security breaches
_logger.LogCritical("Database connection pool exhausted");

// Error: Operation failed, needs investigation
_logger.LogError(ex, "Failed to send email to {Email}", email);

// Warning: Unusual but recoverable
_logger.LogWarning("Notification retry attempt {RetryCount} for {NotificationId}", retryCount, id);

// Information: Important business events
_logger.LogInformation("Notification {NotificationId} sent successfully", id);

// Debug: Diagnostic information
_logger.LogDebug("Processing notification queue. Count: {Count}", notifications.Count);

// Trace: Very detailed diagnostic info
_logger.LogTrace("Query executed: {Query}", sqlQuery);
```

---

## Performance Tips

### String Operations
```csharp
// ✅ CORRECT - StringBuilder for multiple concatenations
var sb = new StringBuilder();
foreach (var notification in notifications)
{
    sb.AppendLine(notification.Subject);
}
var result = sb.ToString();

// ❌ WRONG - string concatenation in loop
var result = "";
foreach (var notification in notifications)
{
    result += notification.Subject + "\n"; // Allocates new string each iteration
}
```

### Collection Initialization
```csharp
// ✅ CORRECT - specify capacity when known
var notifications = new List<Notification>(capacity: 100);
for (int i = 0; i < 100; i++)
{
    notifications.Add(CreateNotification());
}

// ❌ WRONG - no capacity hint
var notifications = new List<Notification>();
for (int i = 0; i < 100; i++)
{
    notifications.Add(CreateNotification()); // May reallocate multiple times
}
```

### LINQ Performance
```csharp
// ✅ CORRECT - filter at database level
var activeNotifications = await _context.Notifications
    .Where(n => n.Status == NotificationStatus.Pending)
    .AsNoTracking()
    .Take(100)
    .ToListAsync();

// ❌ WRONG - load all then filter
var allNotifications = await _context.Notifications.ToListAsync();
var activeNotifications = allNotifications
    .Where(n => n.Status == NotificationStatus.Pending)
    .Take(100)
    .ToList();
```

### Use ValueTask for Synchronous Paths
```csharp
// ✅ CORRECT - no allocation if synchronous
public ValueTask<Notification> GetCachedNotificationAsync(Guid id)
{
    if (_cache.TryGetValue(id, out var notification))
    {
        return new ValueTask<Notification>(notification);
    }

    return new ValueTask<Notification>(FetchFromDatabaseAsync(id));
}

// ✅ CORRECT - standard Task if mostly async
public Task<Notification> GetNotificationAsync(Guid id)
{
    return _repository.GetByIdAsync(id);
}
```

---

## Code Organization

### File Structure
```
src/NotificationSystem.Application/
├── UseCases/
│   ├── Notifications/
│   │   ├── CreateNotification/
│   │   │   ├── CreateNotificationCommand.cs
│   │   │   ├── CreateNotificationCommandHandler.cs
│   │   │   └── CreateNotificationValidator.cs
│   │   └── GetNotifications/
│   │       ├── GetNotificationsQuery.cs
│   │       └── GetNotificationsQueryHandler.cs
├── DTOs/
├── Validators/
└── Common/
```

### Methods per Class
```csharp
// ✅ CORRECT - focused responsibility
public class CreateNotificationCommandHandler
{
    private readonly INotificationRepository _repository;
    private readonly INotificationPublisher _publisher;
    private readonly ILogger<CreateNotificationCommandHandler> _logger;

    public CreateNotificationCommandHandler(
        INotificationRepository repository,
        INotificationPublisher publisher,
        ILogger<CreateNotificationCommandHandler> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result<NotificationDto>> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        // Handle logic
    }
}

// ❌ WRONG - god object
public class NotificationService
{
    public void CreateNotification() { }
    public void SendEmail() { }
    public void ProcessPayment() { }
    public void UpdateUser() { }
    public void GenerateReport() { }
}
```

### Comments & Documentation
```csharp
// ✅ CORRECT - XML documentation for public members
/// <summary>
/// Creates a new notification and publishes it to the message queue.
/// </summary>
/// <param name="command">The command containing notification details.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Result containing the created notification or validation errors.</returns>
/// <exception cref="NotificationException">Thrown when notification creation fails.</exception>
public async Task<Result<NotificationDto>> Handle(
    CreateNotificationCommand command,
    CancellationToken cancellationToken)
{
    // Implementation
}

// ❌ WRONG - inline comments for obvious code
public async Task<Result<NotificationDto>> Handle(CreateNotificationCommand command, CancellationToken cancellationToken)
{
    // Create a new notification
    var notification = new Notification(...);
    
    // Save to database
    await _repository.SaveAsync(notification);
    
    // Return the result
    return Result.Success(new NotificationDto(...));
}
```

---

## Summary Checklist

- ✅ Use PascalCase for types, camelCase for variables
- ✅ Always use nullable reference types enabled
- ✅ Make all I/O operations async
- ✅ Inject dependencies via constructor
- ✅ Use custom exceptions for domain errors
- ✅ Use structured logging with context
- ✅ Filter at database level, not in memory
- ✅ Document public APIs with XML comments
- ✅ Keep classes focused and small
- ✅ Avoid string concatenation in loops
