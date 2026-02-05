using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for AuditLog entity.
/// Configures the audit_logs table with JSONB columns, arrays, and performance indexes.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        // Primary Key
        builder.HasKey(a => a.Id)
            .HasName("pk_audit_logs");

        // Properties
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever(); // Guid is generated in code

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired(false);

        builder.Property(a => a.UserEmail)
            .HasColumnName("user_email")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(a => a.EntityName)
            .HasColumnName("entity_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.ActionType)
            .HasColumnName("action_type")
            .HasMaxLength(50)
            .HasConversion<string>() // Store enum as string
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(a => a.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(a => a.ChangedProperties)
            .HasColumnName("changed_properties")
            .HasColumnType("text[]")
            .IsRequired(false);

        builder.Property(a => a.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired()
            .HasDefaultValueSql("NOW()"); // Database default for UTC now

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45) // IPv6 max length
            .IsRequired(false);

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(a => a.RequestPath)
            .HasColumnName("request_path")
            .HasMaxLength(500)
            .IsRequired(false);

        // Indexes for Performance
        // Single-column indexes for frequent filters
        builder.HasIndex(a => a.EntityName)
            .HasDatabaseName("ix_audit_logs_entity_name")
            .IsUnique(false);

        builder.HasIndex(a => a.EntityId)
            .HasDatabaseName("ix_audit_logs_entity_id")
            .IsUnique(false);

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_audit_logs_user_id")
            .IsUnique(false);

        builder.HasIndex(a => a.ActionType)
            .HasDatabaseName("ix_audit_logs_action_type")
            .IsUnique(false);

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("ix_audit_logs_timestamp")
            .IsDescending() // Descending for ORDER BY optimization
            .IsUnique(false);

        // Composite indexes for common filter combinations
        builder.HasIndex(a => new { a.EntityName, a.EntityId })
            .HasDatabaseName("ix_audit_logs_entity_name_entity_id")
            .IsUnique(false);

        builder.HasIndex(a => new { a.UserId, a.Timestamp })
            .HasDatabaseName("ix_audit_logs_user_id_timestamp")
            .IsDescending(false, true) // Timestamp descending
            .IsUnique(false);

        builder.HasIndex(a => new { a.EntityName, a.ActionType, a.Timestamp })
            .HasDatabaseName("ix_audit_logs_entity_name_action_timestamp")
            .IsDescending(false, false, true) // Timestamp descending
            .IsUnique(false);
    }
}
