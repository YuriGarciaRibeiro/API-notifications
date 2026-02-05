using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Domain.Entities
{
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
        public List<string> ErrorMessages { get; set; } = [];
    }
}

public enum BulkJobStatus
{
    Pending = 0,
    Processing = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}