using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Domain.Entities;

public class RolePermission : IAuditable
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public DateTime GrantedAt { get; set; }
}
