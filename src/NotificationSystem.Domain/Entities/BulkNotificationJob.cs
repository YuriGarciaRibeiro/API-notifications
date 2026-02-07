using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Domain.Entities;

public class BulkNotificationJob : IAuditable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<BulkNotificationItem> Items { get; set; } = [];
    public BulkJobStatus Status { get; set; } = BulkJobStatus.Pending;
    public int TotalCount { get; set; }
    public int ProcessedCount { get; set; } = 0;
    public int SuccessCount { get; set; } = 0;
    public int FailedCount { get; set; } = 0;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public string? RecurringCron { get; set; }
    public string? TimeZone { get; set; } = "UTC";
    public string? HangfireJobId { get; set; }
    public List<string> ErrorMessages { get; set; } = [];

    // Helper properties
    public bool IsRecurring => !string.IsNullOrEmpty(RecurringCron);
    public bool IsScheduled => ScheduledFor.HasValue || IsRecurring;
}


public enum BulkJobStatus
{
    Draft = 0,        // Criado mas não agendado
    Scheduled = 1,    // Agendado para disparo futuro
    Pending = 2,      // Pronto para processar imediatamente
    Processing = 3,   // Sendo processado agora
    Paused = 4,       // Pausado manualmente
    Completed = 5,    // Finalizado
    Failed = 6,       // Falhou
    Cancelled = 7     // Cancelado
}