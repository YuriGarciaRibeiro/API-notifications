# Architecture & Design Patterns - Notification System

**Version**: .NET 10.0  
**Pattern**: Clean Architecture + CQRS + DDD  
**Last Update**: 02/05/2026

---

## 📐 4-Layer Architecture

```
┌─────────────────────────────────────────┐
│   Presentation Layer (API + Workers)    │
│  - Endpoints (Minimal APIs)             │
│  - Authorization & Authentication       │
│  - Request/Response Handling            │
└──────────────┬──────────────────────────┘
               │ (Depends on)
┌──────────────▼──────────────────────────┐
│   Application Layer                     │
│  - Use Cases (Commands/Queries)         │
│  - MediatR Handlers                     │
│  - DTOs & Validators                    │
│  - Event Handlers                       │
└──────────────┬──────────────────────────┘
               │ (Depends on)
┌──────────────▼──────────────────────────┐
│   Infrastructure Layer                  │
│  - EF Core DbContext                    │
│  - Repository Pattern                   │
│  - External Services (Email, SMS)       │
│  - RabbitMQ Communication               │
└──────────────┬──────────────────────────┘
               │ (Depends on)
┌──────────────▼──────────────────────────┐
│   Domain Layer                          │
│  - Entities                             │
│  - Value Objects                        │
│  - Domain Events                        │
│  - Business Rules                       │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

**Domain** (Core Business Logic)
- Entities: `Notification`, `User`, `NotificationChannel`
- Value Objects: `NotificationStatus`, `Email`, `PhoneNumber`
- Events: `NotificationCreated`, `ChannelStatusChanged`
- No external dependencies

**Application** (Use Cases)
- Commands: `CreateNotificationCommand`, `SendNotificationCommand`
- Queries: `GetNotificationsQuery`, `GetNotificationByIdQuery`
- Handlers: Implement use case logic
- Validators: FluentValidation rules
- DTOs: Data transfer objects for API

**Infrastructure** (Technical Implementation)
- EF Core mappings & migrations
- Repository implementations
- External service integrations (SMTP, Twilio)
- RabbitMQ messaging

**Presentation** (API)
- Minimal API endpoints
- Authentication/Authorization
- Request/Response handling
- Health checks

---

## 🎯 CQRS Pattern

**CQRS**: Command Query Responsibility Segregation

### Command Flow (Write)
```
API Endpoint
    ↓
CreateNotificationCommand (Request)
    ↓
ValidationBehavior (FluentValidation)
    ↓
CreateNotificationCommandHandler
    ├─ Create domain entity
    ├─ Apply business rules
    ├─ Publish domain events
    └─ Save to database
    ↓
DomainEventDispatcherBehavior
    ├─ NotificationCreatedEventHandler
    ├─ PublishToRabbitMQHandler
    └─ UpdateMetricsHandler
    ↓
Response: Result<NotificationDto>
```

### Query Flow (Read)
```
API Endpoint
    ↓
GetNotificationsQuery (Request)
    ↓
GetNotificationsQueryHandler
    ├─ Query database (.AsNoTracking())
    ├─ Map to DTOs
    ├─ Apply pagination
    └─ Return results
    ↓
Response: List<NotificationDto>
```

### Command Example
```csharp
// Command: request for operation
public record CreateNotificationCommand(
    string Subject,
    string Body,
    Guid RecipientId,
    NotificationChannel[] Channels
) : IRequest<Result<NotificationDto>>;

// Handler: implements the operation
public class CreateNotificationCommandHandler
    : IRequestHandler<CreateNotificationCommand, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        // Create entity
        var notification = new Notification(
            subject: request.Subject,
            body: request.Body,
            recipientId: request.RecipientId,
            channels: request.Channels
        );

        // Apply business rules
        notification.ValidateChannels();

        // Publish domain events
        notification.AddDomainEvent(new NotificationCreatedEvent(notification));

        // Persist
        await _repository.AddAsync(notification, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(new NotificationDto { ... });
    }
}
```

### Query Example
```csharp
// Query: request for data
public record GetUserNotificationsQuery(
    Guid UserId,
    PaginationOptions Pagination
) : IRequest<PagedResult<NotificationDto>>;

// Handler: retrieves data
public class GetUserNotificationsQueryHandler
    : IRequestHandler<GetUserNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<PagedResult<NotificationDto>> Handle(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.RecipientId == request.UserId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .Select(n => new NotificationDto { ... })
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>(notifications, totalCount);
    }
}
```

---

## 🏛️ Repository Pattern

### Interface Definition
```csharp
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### Implementation
```csharp
public class NotificationRepository : INotificationRepository
{
    private readonly NotificationContext _context;

    public NotificationRepository(NotificationContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Include(n => n.Channels)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Specification Pattern (Optional)
```csharp
public class GetPendingNotificationsSpecification : Specification<Notification>
{
    public GetPendingNotificationsSpecification()
    {
        Query
            .Where(n => n.Status == NotificationStatus.Pending)
            .Include(n => n.Channels)
            .OrderBy(n => n.CreatedAt)
            .Take(100);
    }
}

// Usage
var spec = new GetPendingNotificationsSpecification();
var notifications = await _repository.GetAsync(spec);
```

---

## 📨 Multi-Channel Architecture

### Entity Design (Table-Per-Type - TPT)
```csharp
// Base
public abstract class Notification
{
    public Guid Id { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public Guid RecipientId { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<NotificationChannel> Channels { get; set; }
}

// Derived classes (one per channel)
public class EmailNotification : Notification
{
    public string RecipientEmail { get; set; }
    public string? AttachmentUrls { get; set; }
}

public class SmsNotification : Notification
{
    public string PhoneNumber { get; set; }
    public int RetryCount { get; set; }
}

public class PushNotification : Notification
{
    public string DeviceToken { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}
```

### Database Tables (TPT Inheritance)
```sql
-- Base table
CREATE TABLE notifications (
    id UUID PRIMARY KEY,
    subject VARCHAR(200) NOT NULL,
    body TEXT NOT NULL,
    recipient_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    discriminator VARCHAR(50) NOT NULL
);

-- Derived tables (reference base)
CREATE TABLE email_notifications (
    id UUID PRIMARY KEY REFERENCES notifications(id),
    recipient_email VARCHAR(255) NOT NULL,
    attachment_urls TEXT
);

CREATE TABLE sms_notifications (
    id UUID PRIMARY KEY REFERENCES notifications(id),
    phone_number VARCHAR(20) NOT NULL,
    retry_count INT DEFAULT 0
);

CREATE TABLE push_notifications (
    id UUID PRIMARY KEY REFERENCES notifications(id),
    device_token VARCHAR(255) NOT NULL,
    image_url TEXT,
    data JSONB
);
```

### Channel Strategy Pattern
```csharp
public interface IChannelStrategy
{
    Task SendAsync(Notification notification, CancellationToken cancellationToken);
    bool CanHandle(Notification notification);
}

public class EmailChannelStrategy : IChannelStrategy
{
    public bool CanHandle(Notification notification) => notification is EmailNotification;

    public async Task SendAsync(Notification notification, CancellationToken cancellationToken)
    {
        var emailNotification = (EmailNotification)notification;
        await _emailService.SendAsync(
            emailNotification.RecipientEmail,
            emailNotification.Subject,
            emailNotification.Body,
            cancellationToken
        );
    }
}

public class ChannelStrategyFactory
{
    private readonly IEnumerable<IChannelStrategy> _strategies;

    public ChannelStrategyFactory(IEnumerable<IChannelStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IChannelStrategy GetStrategy(Notification notification)
    {
        return _strategies.FirstOrDefault(s => s.CanHandle(notification))
            ?? throw new InvalidOperationException($"No strategy for {notification.GetType().Name}");
    }
}
```

---

## 🎪 Domain Events

### Event Definition
```csharp
public class NotificationCreatedEvent : DomainEvent
{
    public Guid NotificationId { get; set; }
    public string Subject { get; set; }
    public Guid RecipientId { get; set; }
    public NotificationStatus Status { get; set; }

    public NotificationCreatedEvent(Notification notification)
    {
        NotificationId = notification.Id;
        Subject = notification.Subject;
        RecipientId = notification.RecipientId;
        Status = notification.Status;
    }
}
```

### Event Publishing (Entity)
```csharp
public class Notification
{
    private readonly List<DomainEvent> _domainEvents = new();
    
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // When creating
    public static Notification Create(string subject, string body, Guid recipientId)
    {
        var notification = new Notification { Subject = subject, Body = body, RecipientId = recipientId };
        notification.AddDomainEvent(new NotificationCreatedEvent(notification));
        return notification;
    }
}
```

### Event Dispatcher
```csharp
public class DomainEventDispatcherBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMediator _mediator;
    private readonly IRepository<Entity> _repository;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var entities = await _repository.GetAllAsync();
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var @event in domainEvents)
        {
            await _mediator.Publish(@event, cancellationToken);
        }

        return response;
    }
}
```

### Event Handler
```csharp
public class NotificationCreatedEventHandler
    : INotificationHandler<NotificationCreatedEvent>
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<NotificationCreatedEventHandler> _logger;

    public async Task Handle(NotificationCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification created: {NotificationId}", @event.NotificationId);

        // Publish to RabbitMQ for processing
        await _publisher.PublishAsync(
            exchange: "notifications",
            routingKey: "notification.created",
            message: @event,
            cancellationToken
        );
    }
}
```

---

## 💉 Dependency Injection

### Registration (Program.cs)
```csharp
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(configuration)
    .AddApiServices();

// Extension methods
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(typeof(CreateNotificationCommandHandler).Assembly);
        
        services.AddScoped<IValidator<CreateNotificationCommand>, CreateNotificationValidator>();
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserValidator>();
        
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISmsService, TwilioSmsService>();
        services.AddScoped<IPushService, FirebasePushService>();

        services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => { /* config */ });

        services.AddAuthorization();
        services.AddCors();
        services.AddHealthChecks();

        return services;
    }
}
```

---

## ⚠️ Error Handling

### Result Pattern (Functional)
```csharp
// Base Result
public abstract record Result
{
    public static Result Success() => new SuccessResult();
    public static Result<T> Success<T>(T data) => new SuccessResult<T>(data);
    public static Result Failure(string error) => new FailureResult(error);
    public static Result<T> Failure<T>(string error) => new FailureResult<T>(error);
}

public record SuccessResult : Result;
public record SuccessResult<T> : Result
{
    public T Data { get; init; }
    public SuccessResult(T data) => Data = data;
}

public record FailureResult : Result
{
    public string Error { get; init; }
    public FailureResult(string error) => Error = error;
}

public record FailureResult<T> : Result
{
    public string Error { get; init; }
    public FailureResult(string error) => Error = error;
}
```

### Usage
```csharp
public async Task<Result<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
{
    try
    {
        var notification = new Notification(...);
        await _repository.AddAsync(notification, cancellationToken);
        return Result.Success(new NotificationDto { ... });
    }
    catch (ValidationException ex)
    {
        return Result.Failure<NotificationDto>(ex.Message);
    }
}
```

### Validation Middleware
```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        var response = exception switch
        {
            ValidationException ve => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = ve.Message
            },
            NotificationException ne => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Notification Error",
                Detail = ne.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error"
            }
        };

        context.Response.StatusCode = response.Status.GetValueOrDefault(500);
        await context.Response.WriteAsJsonAsync(response);
    });
});
```

---

## 🧪 Testing Strategy

### Unit Testing (Domain)
```csharp
[TestFixture]
public class NotificationTests
{
    [Test]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var subject = "Test Notification";
        var body = "Test Body";
        var recipientId = Guid.NewGuid();

        // Act
        var notification = Notification.Create(subject, body, recipientId);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(subject, notification.Subject);
        Assert.Equal(body, notification.Body);
        Assert.NotEmpty(notification.DomainEvents);
    }
}
```

### Integration Testing (Repository)
```csharp
[TestFixture]
public class NotificationRepositoryTests
{
    private IServiceProvider _serviceProvider;
    private INotificationRepository _repository;

    [SetUp]
    public void Setup()
    {
        _serviceProvider = new ServiceCollection()
            .AddDbContext<NotificationContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .AddScoped<INotificationRepository, NotificationRepository>()
            .BuildServiceProvider();

        _repository = _serviceProvider.GetRequiredService<INotificationRepository>();
    }

    [Test]
    public async Task AddAsync_ShouldPersistNotification()
    {
        // Arrange
        var notification = Notification.Create("Subject", "Body", Guid.NewGuid());

        // Act
        await _repository.AddAsync(notification);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(notification.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Subject", retrieved.Subject);
    }
}
```

### Endpoint Testing (API)
```csharp
[TestFixture]
public class NotificationEndpointTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task CreateNotification_WithValidRequest_ShouldReturn200()
    {
        // Arrange
        var request = new { subject = "Test", body = "Body" };
        var content = new StringContent(JsonConvert.SerializeObject(request));

        // Act
        var response = await _client.PostAsync("/api/notifications", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

---

## 📈 Scaling Considerations

### Caching Strategy
```csharp
public class GetNotificationByIdQueryHandler
    : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
{
    private readonly INotificationRepository _repository;
    private readonly IDistributedCache _cache;

    public async Task<Result<NotificationDto>> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"notification-{request.Id}";
        
        // Try cache first
        var cached = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cached != null)
        {
            return Result.Success(JsonConvert.DeserializeObject<NotificationDto>(cached));
        }

        // Fallback to database
        var notification = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (notification == null)
        {
            return Result.Failure<NotificationDto>("Notification not found");
        }

        var dto = new NotificationDto { ... };
        await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto)), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
            cancellationToken);

        return Result.Success(dto);
    }
}
```

### Asynchronous Processing
```csharp
// RabbitMQ Worker pattern
public class NotificationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationWorker> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _rabbitMqConnection.CreateModel();
        
        channel.QueueDeclare(queue: "notifications", durable: true);
        var consumer = new AsyncEventingBasicConsumer(channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var notification = JsonConvert.DeserializeObject<NotificationMessage>(body);
                
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<INotificationHandler>();
                await handler.ProcessAsync(notification, stoppingToken);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume(queue: "notifications", autoAck: false, consumer: consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
```

---

## 📚 Summary

- **Clean Architecture**: Separation of concerns across 4 layers
- **CQRS**: Separate read/write models with MediatR
- **Repository Pattern**: Abstract data access
- **Multi-Channel Design**: Extensible with Strategy pattern
- **Domain Events**: Decouple components, enable async processing
- **Dependency Injection**: IoC container for loose coupling
- **Error Handling**: Result pattern for functional error handling
- **Testing**: Unit, integration, and endpoint tests
- **Scaling**: Caching, async workers, database optimization
