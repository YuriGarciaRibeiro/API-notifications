namespace NotificationSystem.Application.Authorization;

/// <summary>
/// Centralized permission constants for authorization policies.
/// </summary>
public static class Permissions
{
    // ========== ROLE PERMISSIONS ==========
    public const string RoleCreate = "role.create";
    public const string RoleUpdate = "role.update";
    public const string RoleDelete = "role.delete";
    public const string RoleView = "role.view";

    // ========== USER PERMISSIONS ==========
    public const string UserCreate = "user.create";
    public const string UserUpdate = "user.update";
    public const string UserDelete = "user.delete";
    public const string UserView = "user.view";
    public const string UserChangePassword = "user.change-password";
    public const string UserAssignRoles = "user.assign-roles";

    // ========== NOTIFICATION PERMISSIONS ==========
    public const string NotificationCreate = "notification.create";
    public const string NotificationView = "notification.view";
    public const string NotificationStats = "notification.stats";
    public const string NotificationDelete = "notification.delete";

    // ========== PROVIDER PERMISSIONS ==========
    public const string ProviderCreate = "provider.create";
    public const string ProviderView = "provider.view";
    public const string ProviderUpdate = "provider.update";
    public const string ProviderDelete = "provider.delete";
    public const string ProviderUpload = "provider.upload";
    public const string ProviderToggle = "provider.toggle";
    public const string ProviderSetPrimary = "provider.set-primary";

    // ========== DEAD LETTER QUEUE PERMISSIONS ==========
    public const string DlqView = "dlq.view";
    public const string DlqReprocess = "dlq.reprocess";
    public const string DlqPurge = "dlq.purge";

    // ========== PERMISSION MANAGEMENT ==========
    public const string PermissionView = "permission.view";

    // ========== AUDIT LOG PERMISSIONS ==========
    public const string AuditView = "audit.view";
    public const string AuditViewAll = "audit.view-all";
    public const string AuditExport = "audit.export";

    // Get all permission constants
    public static IEnumerable<string> GetAll()
    {
        // Roles
        yield return RoleCreate;
        yield return RoleUpdate;
        yield return RoleDelete;
        yield return RoleView;

        // Users
        yield return UserCreate;
        yield return UserUpdate;
        yield return UserDelete;
        yield return UserView;
        yield return UserChangePassword;
        yield return UserAssignRoles;

        // Notifications
        yield return NotificationCreate;
        yield return NotificationView;
        yield return NotificationStats;
        yield return NotificationDelete;

        // Providers
        yield return ProviderCreate;
        yield return ProviderView;
        yield return ProviderUpdate;
        yield return ProviderDelete;
        yield return ProviderUpload;
        yield return ProviderToggle;
        yield return ProviderSetPrimary;

        // Dead Letter Queue
        yield return DlqView;
        yield return DlqReprocess;
        yield return DlqPurge;

        // Permissions
        yield return PermissionView;

        // Audit Logs
        yield return AuditView;
        yield return AuditViewAll;
        yield return AuditExport;
    }
}
