namespace NotificationSystem.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., "notification.send"
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Notification", "User", "Role"
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
