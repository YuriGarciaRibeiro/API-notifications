# 📧 Bulk Notifications - Database-First Architecture

**Última atualização**: 02/05/2026
**Versão**: .NET 10.0
**Padrão**: CQRS + Repository + RabbitMqConsumerBase

---

## 🎯 Visão Geral

Sistema de **envio em massa de notificações** com rastreamento completo de progresso, suporte a retry, cancelamento e resumição.

### Problema Resolvido

```
❌ SEM Bulk:  100.000 messages → RabbitMQ sobrecarregado
✅ COM Bulk: 1 job → BD → Consumer processa gradualmente
```

### Arquitetura de Alto Nível

```
┌─────────────────────────────────────┐
│   POST /api/notifications/bulk      │
│   (CSV, 100.000 destinatários)      │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  CreateBulkNotificationHandler      │
│  (CQRS Command Handler)             │
├─────────────────────────────────────┤
│ 1. Criar BulkNotificationJob (BD)   │
│ 2. Criar BulkNotificationItems (BD) │
│ 3. Publicar msg na fila (ID do job) │
└──────────────┬──────────────────────┘
               │
               ▼
        [RabbitMQ Queue]
          bulk-notifications
               │
               ▼
┌─────────────────────────────────────┐
│  BulkNotificationConsumer           │
│  (extends RabbitMqConsumerBase)     │
├─────────────────────────────────────┤
│ 1. Fetch job do BD                  │
│ 2. For each item:                   │
│    ├─ Criar notificação individual  │
│    ├─ Publicar para fila específica │
│    └─ Atualizar job.ProcessedCount  │
│ 3. Marcar job como Completed        │
└─────────────────────────────────────┘
               │
               ▼
        [RabbitMQ Queues]
   email-notifications │ sms-notifications │ push-notifications
               │
               ▼
    [Email/SMS/Push Consumers]
    (Existentes - sem mudanças!)
```

---

## 📊 Entidades do Banco de Dados

### BulkNotificationJob

```csharp
namespace NotificationSystem.Domain.Entities;

public class BulkNotificationJob : Entity, IAuditable
{
    // Identifiers
    public Guid Id { get; set; }

    // Job Details
    public string Name { get; set; } = string.Empty;                       // "Black Friday 2025"
    public string? Description { get; set; }                               // "50% off campaign"

    // Recipients & Items
    public ICollection<BulkNotificationItem> Items { get; set; } = [];

    // Status Tracking
    public BulkJobStatus Status { get; set; } = BulkJobStatus.Pending;
    public int TotalCount { get; set; }                                    // 100.000
    public int ProcessedCount { get; set; } = 0;                           // Progresso
    public int SuccessCount { get; set; } = 0;
    public int FailureCount { get; set; } = 0;

    // Timing
    public DateTime? StartedAt { get; set; }                               // Quando começou?
    public DateTime? CompletedAt { get; set; }                             // Quando terminou?

    // Audit
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Error Tracking
    public List<string> ErrorMessages { get; set; } = [];                  // Log de erros
}

public enum BulkJobStatus
{
    Pending = 0,        // Criado, aguardando processamento
    Processing = 1,     // Sendo processado
    Completed = 2,      // Finalizou com sucesso
    Failed = 3,         // Falhou totalmente
    Cancelled = 4       // Cancelado pelo usuário
}
```

### BulkNotificationItem

```csharp
public class BulkNotificationItem : Entity, IAuditable
{
    // Identifiers
    public Guid Id { get; set; }
    public Guid BulkJobId { get; set; }

    // Recipient Data
    public string Recipient { get; set; } = string.Empty;                  // email, phone, device_token
    public ChannelType Channel { get; set; }

    // Template Variables {{Name}}, {{Code}}
    public Dictionary<string, string> Variables { get; set; } = [];

    // Status
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }

    // FK para notificação criada
    public Guid? NotificationId { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual BulkNotificationJob BulkJob { get; set; } = null!;
}
```

---

## 🗄️ Migrations

### AddBulkNotificationTables

```bash
dotnet ef migrations add AddBulkNotificationTables \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api
```

**SQL Gerado (PostgreSQL)**:

```sql
-- Tabela principal
CREATE TABLE bulk_notification_jobs (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    total_count INTEGER NOT NULL,
    processed_count INTEGER DEFAULT 0,
    success_count INTEGER DEFAULT 0,
    failure_count INTEGER DEFAULT 0,
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    error_messages TEXT[],
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    updated_by VARCHAR(255),
    CONSTRAINT fk_bulk_jobs_user FOREIGN KEY (created_by_user_id)
        REFERENCES users(id) ON DELETE RESTRICT
);

-- Tabela de items (FK para job)
CREATE TABLE bulk_notification_items (
    id UUID PRIMARY KEY,
    bulk_job_id UUID NOT NULL,
    recipient VARCHAR(500) NOT NULL,
    channel INTEGER NOT NULL,
    variables JSONB,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    error_message TEXT,
    sent_at TIMESTAMP,
    notification_id UUID,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    CONSTRAINT fk_bulk_items_job FOREIGN KEY (bulk_job_id)
        REFERENCES bulk_notification_jobs(id) ON DELETE CASCADE,
    CONSTRAINT fk_bulk_items_notification FOREIGN KEY (notification_id)
        REFERENCES notifications(id) ON DELETE SET NULL
);

-- Índices para performance
CREATE INDEX idx_bulk_jobs_status ON bulk_notification_jobs(status);
CREATE INDEX idx_bulk_jobs_created_by ON bulk_notification_jobs(created_by_user_id);
CREATE INDEX idx_bulk_items_status ON bulk_notification_items(status);
CREATE INDEX idx_bulk_items_job ON bulk_notification_items(bulk_job_id);
CREATE INDEX idx_bulk_items_job_status ON bulk_notification_items(bulk_job_id, status);
```

---

## 🔐 Repository Pattern

### IBulkNotificationRepository

```csharp
namespace NotificationSystem.Application.Interfaces;

public interface IBulkNotificationRepository : IRepository<BulkNotificationJob>
{
    // Get operations
    Task<IEnumerable<BulkNotificationItem>> GetItemsByJobIdAsync(
        Guid jobId,
        NotificationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<BulkNotificationJob?> GetWithItemsAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<int> GetProcessedCountAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    // Update operations
    Task UpdateItemStatusAsync(
        Guid itemId,
        NotificationStatus status,
        string? errorMessage = null,
        Guid? notificationId = null,
        CancellationToken cancellationToken = default);

    Task IncrementProcessedCountAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task UpdateJobStatusAsync(
        Guid jobId,
        BulkJobStatus status,
        CancellationToken cancellationToken = default);

    Task AddErrorMessageAsync(
        Guid jobId,
        string errorMessage,
        CancellationToken cancellationToken = default);

    // Batch operations
    Task AddItemsAsync(
        Guid jobId,
        IEnumerable<BulkNotificationItem> items,
        CancellationToken cancellationToken = default);
}
```

### BulkNotificationRepository (Implementation)

```csharp
namespace NotificationSystem.Infrastructure.Persistence.Repositories;

public class BulkNotificationRepository : Repository<BulkNotificationJob>, IBulkNotificationRepository
{
    public BulkNotificationRepository(NotificationDbContext context) : base(context) { }

    public async Task<IEnumerable<BulkNotificationItem>> GetItemsByJobIdAsync(
        Guid jobId,
        NotificationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BulkNotificationItems
            .Where(x => x.BulkJobId == jobId)
            .AsNoTracking();

        if (status.HasValue)
            query = query.Where(x => x.Status == status);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<BulkNotificationJob?> GetWithItemsAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BulkNotificationJobs
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);
    }

    public async Task<int> GetProcessedCountAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BulkNotificationItems
            .Where(x => x.BulkJobId == jobId && x.Status != NotificationStatus.Pending)
            .CountAsync(cancellationToken);
    }

    public async Task UpdateItemStatusAsync(
        Guid itemId,
        NotificationStatus status,
        string? errorMessage = null,
        Guid? notificationId = null,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.BulkNotificationItems.FindAsync(
            new object?[] { itemId },
            cancellationToken: cancellationToken);

        if (item != null)
        {
            item.Status = status;
            item.ErrorMessage = errorMessage;
            item.UpdatedAt = DateTime.UtcNow;

            if (status == NotificationStatus.Sent)
                item.SentAt = DateTime.UtcNow;

            if (notificationId.HasValue)
                item.NotificationId = notificationId;

            _context.BulkNotificationItems.Update(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task IncrementProcessedCountAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.BulkNotificationJobs.FindAsync(
            new object?[] { jobId },
            cancellationToken: cancellationToken);

        if (job != null)
        {
            job.ProcessedCount++;
            job.UpdatedAt = DateTime.UtcNow;
            _context.BulkNotificationJobs.Update(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateJobStatusAsync(
        Guid jobId,
        BulkJobStatus status,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.BulkNotificationJobs.FindAsync(
            new object?[] { jobId },
            cancellationToken: cancellationToken);

        if (job != null)
        {
            job.Status = status;
            job.UpdatedAt = DateTime.UtcNow;

            if (status == BulkJobStatus.Processing && !job.StartedAt.HasValue)
                job.StartedAt = DateTime.UtcNow;

            if (status == BulkJobStatus.Completed && !job.CompletedAt.HasValue)
                job.CompletedAt = DateTime.UtcNow;

            _context.BulkNotificationJobs.Update(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddErrorMessageAsync(
        Guid jobId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.BulkNotificationJobs.FindAsync(
            new object?[] { jobId },
            cancellationToken: cancellationToken);

        if (job != null)
        {
            job.ErrorMessages.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {errorMessage}");
            job.UpdatedAt = DateTime.UtcNow;
            _context.BulkNotificationJobs.Update(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddItemsAsync(
        Guid jobId,
        IEnumerable<BulkNotificationItem> items,
        CancellationToken cancellationToken = default)
    {
        var itemsList = items.ToList();
        await _context.BulkNotificationItems.AddRangeAsync(itemsList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

## 📨 Message Classes

### BulkNotificationJobMessage

```csharp
namespace NotificationSystem.Application.Messages;

/// <summary>
/// Mensagem publicada quando um bulk job é criado.
/// Consumer fetcha job do BD e processa items.
/// </summary>
public record BulkNotificationJobMessage(
    Guid JobId
);
```

---

## 🎯 CQRS: Commands

### CreateBulkNotificationCommand

```csharp
namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

/// <summary>
/// Comando para criar um bulk job de notificações.
/// </summary>
public record CreateBulkNotificationCommand(
    string Name,
    string? Description,
    List<CreateBulkNotificationItemRequest> Items
) : IRequest<Result<BulkNotificationJobResponse>>;

public record CreateBulkNotificationItemRequest(
    string Recipient,
    ChannelType Channel,
    Dictionary<string, string>? Variables = null
);

public record BulkNotificationJobResponse(
    Guid Id,
    string Name,
    int TotalCount,
    string Status
);
```

### CreateBulkNotificationValidator

```csharp
namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

public class CreateBulkNotificationValidator
    : AbstractValidator<CreateBulkNotificationCommand>
{
    public CreateBulkNotificationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Bulk job name is required")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one recipient is required")
            .Must(x => x.Count <= 1000000)
            .WithMessage("Cannot create bulk job with more than 1,000,000 recipients");

        RuleFor(x => x.Items)
            .ForEach(item =>
            {
                item.RuleFor(x => x.Recipient)
                    .NotEmpty().WithMessage("Recipient cannot be empty");

                item.RuleFor(x => x.Channel)
                    .IsInEnum().WithMessage("Invalid channel type");
            });
    }
}
```

### CreateBulkNotificationHandler

```csharp
namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

public class CreateBulkNotificationHandler
    : IRequestHandler<CreateBulkNotificationCommand, Result<BulkNotificationJobResponse>>
{
    private readonly IBulkNotificationRepository _bulkRepository;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly ILogger<CreateBulkNotificationHandler> _logger;
    private readonly ICurrentUser _currentUser;

    public CreateBulkNotificationHandler(
        IBulkNotificationRepository bulkRepository,
        IRabbitMqPublisher rabbitMqPublisher,
        ILogger<CreateBulkNotificationHandler> logger,
        ICurrentUser currentUser)
    {
        _bulkRepository = bulkRepository ?? throw new ArgumentNullException(nameof(bulkRepository));
        _rabbitMqPublisher = rabbitMqPublisher ?? throw new ArgumentNullException(nameof(rabbitMqPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<BulkNotificationJobResponse>> Handle(
        CreateBulkNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating bulk notification job '{JobName}' with {ItemCount} items",
                request.Name,
                request.Items.Count);

            // 1. Criar BulkNotificationJob
            var job = new BulkNotificationJob
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Status = BulkJobStatus.Pending,
                TotalCount = request.Items.Count,
                ProcessedCount = 0,
                CreatedByUserId = _currentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            // 2. Criar items e associar ao job
            var items = request.Items
                .Select(item => new BulkNotificationItem
                {
                    Id = Guid.NewGuid(),
                    BulkJobId = job.Id,
                    Recipient = item.Recipient,
                    Channel = item.Channel,
                    Variables = item.Variables ?? new(),
                    Status = NotificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            job.Items = items;

            // 3. Salvar job e items no BD
            await _bulkRepository.AddAsync(job, cancellationToken);
            await _bulkRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Bulk notification job '{JobId}' created successfully with {ItemCount} items",
                job.Id,
                job.Items.Count);

            // 4. Publicar mensagem na fila RabbitMQ (apenas ID)
            var message = new BulkNotificationJobMessage(job.Id);
            await _rabbitMqPublisher.PublishAsync(
                "bulk-notifications",
                message,
                cancellationToken);

            _logger.LogInformation(
                "Bulk notification job '{JobId}' published to RabbitMQ for processing",
                job.Id);

            // 5. Retornar resposta
            return Result.Ok(new BulkNotificationJobResponse(
                job.Id,
                job.Name,
                job.TotalCount,
                job.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk notification job");
            return Result.Fail(new Error("BulkNotificationCreationFailed")
                .CausedBy(ex));
        }
    }
}
```

---

## 🔄 Consumer: BulkNotificationConsumer

### BulkNotificationMessage (Application Layer)

```csharp
namespace NotificationSystem.Worker.Bulk;

/// <summary>
/// Worker específico para processar bulk jobs.
/// Herda de RabbitMqConsumerBase para reutilizar infraestrutura.
/// </summary>
public class Worker : RabbitMqConsumerBase<BulkNotificationJobMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    protected override string QueueName => "bulk-notifications";

    public Worker(
        ILogger<Worker> logger,
        IOptions<RabbitMqSettings> rabbitMqOptions,
        IServiceProvider serviceProvider,
        MessageProcessingMiddleware<BulkNotificationJobMessage> middleware)
        : base(logger, rabbitMqOptions, serviceProvider, middleware)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Processamento principal do bulk job.
    /// Chamado pelo middleware com retry automático.
    /// </summary>
    protected override async Task ProcessMessageAsync(
        BulkNotificationJobMessage message,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var bulkRepository = scope.ServiceProvider.GetRequiredService<IBulkNotificationRepository>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var rabbitMqPublisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

        _logger.LogInformation("Starting bulk job processing: {JobId}", message.JobId);

        // 1. Buscar job completo do BD (com items)
        var job = await bulkRepository.GetWithItemsAsync(message.JobId, cancellationToken);

        if (job == null)
        {
            _logger.LogError("Bulk job {JobId} not found. Job may have been deleted.", message.JobId);
            throw new InvalidOperationException($"Bulk job {message.JobId} not found");
        }

        if (job.Status == BulkJobStatus.Cancelled)
        {
            _logger.LogInformation("Bulk job {JobId} is cancelled. Skipping processing.", message.JobId);
            return;
        }

        // 2. Marcar job como Processing
        job.Status = BulkJobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        await bulkRepository.UpdateJobStatusAsync(job.Id, job.Status, cancellationToken);

        _logger.LogInformation(
            "Bulk job {JobId} status changed to Processing. Total items: {TotalCount}",
            job.Id,
            job.TotalCount);

        int successCount = 0;
        int failureCount = 0;

        // 3. Processar cada item
        foreach (var item in job.Items)
        {
            // Skip se já foi processado
            if (item.Status != NotificationStatus.Pending)
            {
                _logger.LogDebug(
                    "Item {ItemId} already processed with status {Status}. Skipping.",
                    item.Id,
                    item.Status);
                continue;
            }

            try
            {
                // Criar notificação individual
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = job.CreatedByUserId,  // Quem enviou o bulk
                    CreatedAt = DateTime.UtcNow,
                    Channels = new()
                };

                // Criar channel específico baseado no tipo
                NotificationChannel? channel = item.Channel switch
                {
                    ChannelType.Email => new EmailChannel
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        To = item.Recipient,
                        Subject = "Notification",
                        Body = "Check your notification",
                        IsBodyHtml = false
                    },

                    ChannelType.Sms => new SmsChannel
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        To = item.Recipient,
                        Message = "You have a new notification"
                    },

                    ChannelType.Push => new PushChannel
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        To = item.Recipient,
                        Content = new NotificationContent
                        {
                            Title = "Notification",
                            Body = "You have a new notification"
                        },
                        Data = item.Variables ?? new(),
                        Platform = "fcm"
                    },

                    _ => null
                };

                if (channel == null)
                {
                    throw new InvalidOperationException($"Unsupported channel type: {item.Channel}");
                }

                notification.Channels.Add(channel);

                // Salvar notificação no BD
                await notificationRepository.AddAsync(notification, cancellationToken);
                await notificationRepository.SaveChangesAsync(cancellationToken);

                // Publicar mensagem para o consumer específico do canal
                var channelMessage = item.Channel switch
                {
                    ChannelType.Email => new EmailChannelMessage(
                        channel.Id,
                        notification.Id,
                        ((EmailChannel)channel).To,
                        ((EmailChannel)channel).Subject,
                        ((EmailChannel)channel).Body,
                        ((EmailChannel)channel).IsBodyHtml
                    ),

                    ChannelType.Sms => new SmsChannelMessage(
                        channel.Id,
                        notification.Id,
                        ((SmsChannel)channel).To,
                        ((SmsChannel)channel).Message,
                        ((SmsChannel)channel).SenderId
                    ),

                    _ => null
                };

                if (channelMessage != null)
                {
                    string queueName = item.Channel switch
                    {
                        ChannelType.Email => "email-notifications",
                        ChannelType.Sms => "sms-notifications",
                        ChannelType.Push => "push-notifications",
                        _ => throw new InvalidOperationException()
                    };

                    await rabbitMqPublisher.PublishAsync(queueName, channelMessage, cancellationToken);
                }

                // Atualizar item como enviado
                await bulkRepository.UpdateItemStatusAsync(
                    item.Id,
                    NotificationStatus.Sent,
                    notificationId: notification.Id,
                    cancellationToken: cancellationToken);

                successCount++;

                _logger.LogDebug(
                    "Bulk item {ItemId} processed successfully. Notification {NotificationId} created.",
                    item.Id,
                    notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing bulk item {ItemId} for job {JobId}",
                    item.Id,
                    job.Id);

                // Atualizar item como falhado
                await bulkRepository.UpdateItemStatusAsync(
                    item.Id,
                    NotificationStatus.Failed,
                    errorMessage: ex.Message,
                    cancellationToken: cancellationToken);

                failureCount++;

                // Adicionar erro ao job
                await bulkRepository.AddErrorMessageAsync(
                    job.Id,
                    $"Item {item.Recipient}: {ex.Message}",
                    cancellationToken);
            }

            // Atualizar progresso a cada item processado
            job.ProcessedCount++;
            job.SuccessCount = successCount;
            job.FailureCount = failureCount;
            await bulkRepository.UpdateJobStatusAsync(job.Id, job.Status, cancellationToken);

            // Log de progresso a cada 100 items
            if (job.ProcessedCount % 100 == 0)
            {
                double percentComplete = (job.ProcessedCount / (double)job.TotalCount) * 100;
                _logger.LogInformation(
                    "Bulk job {JobId} progress: {Processed}/{Total} ({Percent:F2}%)",
                    job.Id,
                    job.ProcessedCount,
                    job.TotalCount,
                    percentComplete);
            }
        }

        // 4. Finalizar job
        job.Status = BulkJobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.SuccessCount = successCount;
        job.FailureCount = failureCount;

        await bulkRepository.UpdateJobStatusAsync(job.Id, job.Status, cancellationToken);

        _logger.LogInformation(
            "Bulk job {JobId} completed: {Success} success, {Failure} failures, Duration: {Duration}s",
            job.Id,
            successCount,
            failureCount,
            (DateTime.UtcNow - job.StartedAt!.Value).TotalSeconds);
    }

    /// <summary>
    /// Requerido pela interface RabbitMqConsumerBase.
    /// Não é usado em bulk processing (não temos notificationId na mensagem).
    /// </summary>
    protected override Task<(Guid NotificationId, Guid ChannelId)> GetNotificationIdsAsync(
        BulkNotificationJobMessage message)
    {
        // Retorna valores dummy - não usado pois middleware não atualiza status
        return Task.FromResult((Guid.Empty, Guid.Empty));
    }

    /// <summary>
    /// Requerido pela interface RabbitMqConsumerBase.
    /// Retorna Notification para conformidade, mas não é relevante.
    /// </summary>
    protected override Type GetChannelType()
    {
        return typeof(EmailChannel);  // Dummy - não relevante
    }
}
```

### Program.cs (Bulk Consumer)

```csharp
// src/Consumers/NotificationSystem.Consumer.Bulk/Program.cs

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Consumers;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Infrastructure;
using NotificationSystem.Infrastructure.Persistence;
using NotificationSystem.Infrastructure.Persistence.Repositories;
using NotificationSystem.Infrastructure.Services;
using NotificationSystem.Worker.Bulk;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

// Configure Options
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(DatabaseSettings.SectionName));

// Database
builder.Services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
{
    var databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    if (string.IsNullOrEmpty(databaseSettings.ConnectionString))
    {
        throw new InvalidOperationException("Database connection string not found.");
    }

    options.UseNpgsql(
        databaseSettings.ConnectionString,
        b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)
    );
});

// Repositories
builder.Services.AddScoped<IBulkNotificationRepository, BulkNotificationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Retry Strategy
builder.Services.AddSingleton<IRetryStrategy>(sp =>
    new ExponentialBackoffRetryStrategy(
        maxRetries: 3,
        initialDelay: TimeSpan.FromSeconds(2),
        maxDelay: TimeSpan.FromMinutes(5)));

builder.Services.AddSingleton<MessageProcessingMiddleware<BulkNotificationJobMessage>>();

// RabbitMQ Publisher
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

// Register Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
```

---

## 🛣️ API Endpoints

### Criar Bulk Job

```csharp
// POST /api/notifications/bulk
public static class BulkNotificationEndpoints
{
    public static IEndpointRouteBuilder MapBulkNotificationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications/bulk")
            .WithTags("Bulk Notifications")
            .RequireAuthorization(Permissions.BulkNotificationCreate);

        // Criar bulk job
        group.MapPost("/",
            async ([FromBody] CreateBulkNotificationCommand command,
                   IMediator mediator,
                   CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("CreateBulkNotification")
            .WithSummary("Create bulk notification job")
            .WithDescription("Create a bulk job to send notifications to multiple recipients")
            .Produces<BulkNotificationJobResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // Obter progresso do job
        group.MapGet("/{jobId:guid}/progress",
            async (Guid jobId,
                   IBulkNotificationRepository repo,
                   CancellationToken cancellationToken) =>
            {
                var job = await repo.GetByIdAsync(jobId, cancellationToken);

                if (job == null)
                    return Results.NotFound();

                var percentComplete = job.TotalCount > 0
                    ? (job.ProcessedCount / (double)job.TotalCount * 100)
                    : 0;

                return Results.Ok(new
                {
                    jobId = job.Id,
                    name = job.Name,
                    status = job.Status.ToString(),
                    totalCount = job.TotalCount,
                    processedCount = job.ProcessedCount,
                    successCount = job.SuccessCount,
                    failureCount = job.FailureCount,
                    percentComplete = percentComplete.ToString("F2") + "%",
                    startedAt = job.StartedAt,
                    completedAt = job.CompletedAt,
                    createdAt = job.CreatedAt
                });
            })
            .WithName("GetBulkProgress")
            .WithSummary("Get bulk job progress")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Cancelar bulk job
        group.MapDelete("/{jobId:guid}",
            async (Guid jobId,
                   IBulkNotificationRepository repo,
                   ILogger<Program> logger,
                   CancellationToken cancellationToken) =>
            {
                var job = await repo.GetByIdAsync(jobId, cancellationToken);

                if (job == null)
                    return Results.NotFound();

                if (job.Status == BulkJobStatus.Completed ||
                    job.Status == BulkJobStatus.Failed)
                {
                    return Results.BadRequest(
                        "Cannot cancel a job that has already completed or failed");
                }

                job.Status = BulkJobStatus.Cancelled;
                job.UpdatedAt = DateTime.UtcNow;

                await repo.UpdateAsync(job, cancellationToken);
                await repo.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Bulk job {JobId} cancelled by user", jobId);

                return Results.NoContent();
            })
            .WithName("CancelBulkJob")
            .WithSummary("Cancel bulk notification job")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        // Listar items de um job (com filtro de status)
        group.MapGet("/{jobId:guid}/items",
            async (Guid jobId,
                   [FromQuery] NotificationStatus? status,
                   IBulkNotificationRepository repo,
                   CancellationToken cancellationToken) =>
            {
                var items = await repo.GetItemsByJobIdAsync(jobId, status, cancellationToken);

                if (!items.Any())
                    return Results.NotFound();

                return Results.Ok(items.Select(x => new
                {
                    id = x.Id,
                    recipient = x.Recipient,
                    channel = x.Channel.ToString(),
                    status = x.Status.ToString(),
                    errorMessage = x.ErrorMessage,
                    sentAt = x.SentAt
                }));
            })
            .WithName("GetBulkItems")
            .WithSummary("Get bulk job items")
            .WithDescription("List all items in a bulk job, optionally filtered by status")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }
}

// Program.cs: registrar endpoints
app.MapBulkNotificationEndpoints();
```

---

## 📊 Fluxo Completo (Diagrama)

```
┌─────────────────────────────────────────────────────────────────┐
│ Cliente faz request                                             │
│ POST /api/notifications/bulk                                    │
│ ├─ name: "Black Friday 2025"                                    │
│ └─ items: [100.000 recipients]                                  │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────────────────────────────────┐
│ API: CreateBulkNotificationHandler                              │
├──────────────────────────────────────────────────────────────────┤
│ 1. Validar request (FluentValidation)                           │
│ 2. Criar BulkNotificationJob (BD)                               │
│    ├─ status = Pending                                          │
│    ├─ totalCount = 100.000                                      │
│    └─ processedCount = 0                                        │
│ 3. Criar BulkNotificationItems (BD) x 100.000                  │
│    ├─ recipient = email                                         │
│    ├─ status = Pending                                          │
│    └─ variables = {Name, Code, etc}                             │
│ 4. Publicar BulkNotificationJobMessage (JobId)                 │
│    └─ RabbitMQ: bulk-notifications queue                        │
│ 5. Return 201 Created com JobId                                │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ▼
        ┌──────────────────────────┐
        │   RabbitMQ Queue         │
        │ bulk-notifications       │
        │  1 message (JobId)       │
        └──────────────┬───────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│ Consumer: BulkNotificationConsumer                               │
├──────────────────────────────────────────────────────────────────┤
│ 1. Receber BulkNotificationJobMessage(JobId)                    │
│ 2. Fetch BulkNotificationJob + Items do BD                      │
│ 3. Marcar job status = Processing                               │
│ 4. For each BulkNotificationItem (100.000):                     │
│    ├─ Resolver variáveis do template                            │
│    ├─ Criar Notification individual                             │
│    ├─ Criar Channel (Email/SMS/Push)                            │
│    ├─ Salvar no BD                                              │
│    ├─ Publicar EmailChannelMessage/SmsChannelMessage (x3)      │
│    ├─ Update item status = Sent                                 │
│    ├─ Increment job.ProcessedCount (com log a cada 100)        │
│    └─ [Loop continua...]                                        │
│ 5. Set job status = Completed, completedAt = now                │
│ 6. Ack message no RabbitMQ                                       │
└──────────────────┬──────────────────────────────────────────────┘
                   │
           ┌───────┼───────┐
           │       │       │
           ▼       ▼       ▼
        ┌─────┐ ┌──────┐ ┌─────┐
        │Email│ │ SMS  │ │Push │
        │Queue│ │Queue │ │Queue│
        └──┬──┘ └──┬───┘ └──┬──┘
           │       │        │
           ▼       ▼        ▼
    ┌────────────────────────────┐
    │  Email/SMS/Push Consumers   │
    │  (Existentes, sem mudanças) │
    └──────────┬─────────────────┘
               │
               ▼
        ┌────────────────┐
        │ External APIs  │
        │ SMTP/Twilio/.. │
        └────────────────┘


MONITORAMENTO EM TEMPO REAL:
┌─────────────────────────────────┐
│ Client monitorando progresso     │
├─────────────────────────────────┤
│ GET /api/notifications/bulk/{id}/progress
│
│ Response:
│ {
│   "status": "Processing",
│   "processedCount": 45000,
│   "totalCount": 100000,
│   "percentComplete": "45.00%",
│   "successCount": 44500,
│   "failureCount": 500
│ }
│
│ ✅ Pode cancelar: DELETE /api/notifications/bulk/{id}
│ ✅ Pode ver falhas: GET /api/notifications/bulk/{id}/items?status=Failed
└─────────────────────────────────┘
```

---

## 🎯 Queries (Read Operations)

### GetBulkNotificationJobQuery

```csharp
namespace NotificationSystem.Application.UseCases.GetBulkNotificationJob;

public record GetBulkNotificationJobQuery(Guid JobId)
    : IRequest<Result<BulkNotificationJobDetailResponse>>;

public record BulkNotificationJobDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int TotalCount,
    int ProcessedCount,
    int SuccessCount,
    int FailureCount,
    double PercentComplete,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    List<string> ErrorMessages
);

public class GetBulkNotificationJobHandler
    : IRequestHandler<GetBulkNotificationJobQuery, Result<BulkNotificationJobDetailResponse>>
{
    private readonly IBulkNotificationRepository _repository;

    public async Task<Result<BulkNotificationJobDetailResponse>> Handle(
        GetBulkNotificationJobQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);

        if (job == null)
            return Result.Fail(new Error("NotFound"));

        var percentComplete = job.TotalCount > 0
            ? (job.ProcessedCount / (double)job.TotalCount * 100)
            : 0;

        var response = new BulkNotificationJobDetailResponse(
            job.Id,
            job.Name,
            job.Description,
            job.Status.ToString(),
            job.TotalCount,
            job.ProcessedCount,
            job.SuccessCount,
            job.FailureCount,
            percentComplete,
            job.StartedAt,
            job.CompletedAt,
            job.CreatedAt,
            job.ErrorMessages);

        return Result.Ok(response);
    }
}
```

---

## 🔐 Permissions

```csharp
namespace NotificationSystem.Application.Authorization;

public static class Permissions
{
    // Existentes
    public const string NotificationView = "notification.view";
    public const string NotificationCreate = "notification.create";

    // Novo: Bulk operations
    public const string BulkNotificationCreate = "bulk-notification.create";
    public const string BulkNotificationView = "bulk-notification.view";
    public const string BulkNotificationCancel = "bulk-notification.cancel";
}
```

---

## ⚙️ Configuração (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "NotificationSystem.Worker.Bulk": "Debug"
    }
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=NotificationSystem;Username=admin;Password=password;Pooling=true;"
  }
}
```

---

## 📈 Performance & Escalabilidade

### Otimizações Implementadas

| Aspecto | Implementação |
|---------|--------------|
| **Batch Inserts** | Items criados em batch no BD |
| **Async/Await** | Tudo é não-bloqueante |
| **Connection Pooling** | EF Core com pool de conexões |
| **Index Strategy** | Índices em (job_id, status) |
| **Retry Logic** | ExponentialBackoff com config por consumer |
| **DLQ** | Mensagens falhadas vão para Dead Letter Queue |

### Limites Recomendados

```
- Máximo items por bulk: 1.000.000
- Máximo concurrent bulk jobs: 10-20
- Recommended batch size: 500-1000 items/segundo
- Consumer parallelism: 5-10 workers
```

---

## 🧪 Exemplo de Uso (cURL)

### 1. Criar Bulk Job

```bash
curl -X POST http://localhost:5000/api/notifications/bulk \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Black Friday Campaign",
    "description": "50% off promotion",
    "items": [
      {
        "recipient": "user1@example.com",
        "channel": 0,
        "variables": {
          "name": "João",
          "discount": "50%"
        }
      },
      {
        "recipient": "+5511999999999",
        "channel": 1,
        "variables": {
          "name": "Maria",
          "code": "BLACKFRIDAY50"
        }
      }
    ]
  }'
```

**Resposta (201 Created)**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Black Friday Campaign",
  "totalCount": 2,
  "status": "Pending"
}
```

### 2. Monitorar Progresso

```bash
curl http://localhost:5000/api/notifications/bulk/550e8400-e29b-41d4-a716-446655440001/progress \
  -H "Authorization: Bearer $TOKEN"
```

**Resposta**:
```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Black Friday Campaign",
  "status": "Processing",
  "totalCount": 100000,
  "processedCount": 45000,
  "successCount": 44500,
  "failureCount": 500,
  "percentComplete": "45.00%",
  "startedAt": "2025-02-10T09:00:30Z",
  "createdAt": "2025-02-10T08:59:00Z"
}
```

### 3. Obter Items com Erro

```bash
curl "http://localhost:5000/api/notifications/bulk/550e8400.../items?status=Failed" \
  -H "Authorization: Bearer $TOKEN"
```

### 4. Cancelar Job

```bash
curl -X DELETE http://localhost:5000/api/notifications/bulk/550e8400-e29b-41d4-a716-446655440001 \
  -H "Authorization: Bearer $TOKEN"
```

---

## 📋 Checklist de Implementação

- [ ] Criar entidades `BulkNotificationJob` e `BulkNotificationItem`
- [ ] Criar interface `IBulkNotificationRepository`
- [ ] Implementar `BulkNotificationRepository`
- [ ] Criar migration do banco
- [ ] Criar `BulkNotificationJobMessage` (record)
- [ ] Criar `CreateBulkNotificationCommand` e Validator
- [ ] Implementar `CreateBulkNotificationHandler`
- [ ] Criar `BulkNotificationConsumer` (Worker)
- [ ] Configurar DI do Consumer (Program.cs)
- [ ] Criar `BulkNotificationEndpoints`
- [ ] Adicionar permissões para Bulk Operations
- [ ] Testes unitários do handler
- [ ] Testes de integração do consumer
- [ ] Documentação OpenAPI/Swagger
- [ ] Testes de carga (1M recipients)

---

## 🔗 Referências

- [RabbitMqConsumerBase](../src/NotificationSystem.Application/Consumers/RabbitMqConsumerBase.cs)
- [MessageProcessingMiddleware](../src/NotificationSystem.Application/Consumers/MessageProcessingMiddleware.cs)
- [NotificationEndpoints](../src/NotificationSystem.Api/Endpoints/NotificationEndpoints.cs)
- [Email Consumer Pattern](../src/Consumers/NotificationSystem.Consumer.Email/Worker.cs)
