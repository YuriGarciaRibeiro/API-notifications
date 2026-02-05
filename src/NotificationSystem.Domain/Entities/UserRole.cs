using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Domain.Entities;

public class UserRole : IAuditable
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
}
