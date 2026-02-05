using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Interfaces;

namespace NotificationSystem.Infrastructure.Persistence.Interceptors;

/// <summary>
/// SaveChangesInterceptor that automatically captures audit logs for all entity changes.
/// Runs before SaveChanges to intercept Created, Updated, and Deleted operations.
/// </summary>
public class AuditLogInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> ExcludedProperties = new()
    {
        "PasswordHash",
        "RefreshToken",
        "SecretKey",
        "Token"
    };

    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogInterceptor(
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Async interceptor for SaveChanges operations.
    /// Captures entity changes and adds them as audit log entries.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        CreateAndAddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Sync interceptor for SaveChanges operations.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        CreateAndAddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Creates audit log entries for all entity changes and adds them to the context.
    /// </summary>
    private void CreateAndAddAuditLogs(DbContext context)
    {
        var auditLogs = CreateAuditLogs(context);

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    /// <summary>
    /// Analyzes the change tracker and creates AuditLog entries for each entity change.
    /// </summary>
    private List<AuditLog> CreateAuditLogs(DbContext context)
    {
        var auditLogs = new List<AuditLog>();

        // Capture user context
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;

        // Capture request context
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = GetClientIpAddress(httpContext);
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();
        var requestPath = httpContext?.Request.Path.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip non-auditable entities (only audit entities that implement IAuditable)
            if (entry.Entity is not IAuditable)
                continue;

            // Skip entities with no state change
            if (entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                continue;

            var entityName = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);

            if (string.IsNullOrEmpty(entityId))
                continue;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserEmail = userEmail,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                RequestPath = requestPath
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditLog.ActionType = AuditAction.Created;
                    auditLog.NewValues = SerializeEntity(entry.CurrentValues);
                    break;

                case EntityState.Modified:
                    auditLog.ActionType = AuditAction.Updated;
                    auditLog.OldValues = SerializeEntity(entry.OriginalValues);
                    auditLog.NewValues = SerializeEntity(entry.CurrentValues);
                    auditLog.ChangedProperties = GetChangedProperties(entry);
                    break;

                case EntityState.Deleted:
                    auditLog.ActionType = AuditAction.Deleted;
                    auditLog.OldValues = SerializeEntity(entry.OriginalValues);
                    break;

                // AddedToContext and Modified are captured, others are ignored
            }

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    /// <summary>
    /// Extracts the entity's ID from the change tracker entry.
    /// Looks for "Id" property or primary key property.
    /// </summary>
    private static string GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p =>
            p.Metadata.Name == "Id" || p.Metadata.IsPrimaryKey());

        return idProperty?.CurrentValue?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Serializes entity property values to JSON, excluding sensitive fields and shadow properties.
    /// </summary>
    private static string? SerializeEntity(PropertyValues propertyValues)
    {
        var properties = propertyValues.Properties
            .Where(p =>
                !p.IsShadowProperty() && // Skip EF Core shadow properties
                !ExcludedProperties.Contains(p.Name)) // Skip sensitive fields
            .ToDictionary(
                p => p.Name,
                p => propertyValues[p]
            );

        if (properties.Count == 0)
            return null;

        return JsonSerializer.Serialize(properties, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Gets list of property names that were modified in the entity.
    /// Only called for Updated state entries.
    /// </summary>
    private static List<string> GetChangedProperties(EntityEntry entry)
    {
        return entry.Properties
            .Where(p => p.IsModified && !p.Metadata.IsShadowProperty())
            .Select(p => p.Metadata.Name)
            .ToList();
    }

    /// <summary>
    /// Gets the client IP address, accounting for reverse proxies using X-Forwarded-For header.
    /// Priority: X-Forwarded-For header > RemoteIpAddress
    /// </summary>
    private static string? GetClientIpAddress(HttpContext? context)
    {
        if (context is null)
            return null;

        // Check for X-Forwarded-For header (set by reverse proxies/load balancers)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
