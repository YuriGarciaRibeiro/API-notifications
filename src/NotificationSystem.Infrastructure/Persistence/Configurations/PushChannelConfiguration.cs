using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;
using System.Text.Json;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public class PushChannelConfiguration : IEntityTypeConfiguration<PushChannel>
{
    public void Configure(EntityTypeBuilder<PushChannel> builder)
    {
        builder.Property(p => p.To)
            .HasColumnName("to")
            .IsRequired()
            .HasMaxLength(500);

        builder.OwnsOne(p => p.Content, content =>
        {
            content.Property(c => c.Title)
                .HasColumnName("content_title")
                .HasMaxLength(100);

            content.Property(c => c.Body)
                .HasColumnName("content_body")
                .HasMaxLength(500);

            content.Property(c => c.ClickAction)
                .HasColumnName("content_click_action")
                .HasMaxLength(200);
        });

        builder.Property(p => p.Data)
            .HasColumnName("data")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
                     ?? new Dictionary<string, string>()
            )
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToDictionary(k => k.Key, v => v.Value)
            ));

        builder.OwnsOne(p => p.Android, android =>
        {
            android.ToJson("android_config");
            android.Property(a => a.Priority).HasMaxLength(20);
            android.Property(a => a.Ttl).HasMaxLength(20);
        });

        builder.OwnsOne(p => p.Apns, apns =>
        {
            apns.ToJson("apns_config");
            apns.Property(a => a.Headers)
                .HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) : null,
                    v => v != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) : null
                )
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>?>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? null : c.ToDictionary(k => k.Key, v => v.Value)
                ));
        });

        builder.OwnsOne(p => p.Webpush, webpush =>
        {
            webpush.ToJson("webpush_config");
            webpush.Property(w => w.Headers)
                .HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) : null,
                    v => v != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) : null
                )
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>?>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? null : c.ToDictionary(k => k.Key, v => v.Value)
                ));
        });

        builder.Property(p => p.Condition)
            .HasColumnName("condition")
            .HasMaxLength(200);

        builder.Property(p => p.TimeToLive)
            .HasColumnName("time_to_live");

        builder.Property(p => p.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20);

        builder.Property(p => p.MutableContent)
            .HasColumnName("mutable_content");

        builder.Property(p => p.ContentAvailable)
            .HasColumnName("content_available");

        builder.Property(p => p.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false);
    }
}
