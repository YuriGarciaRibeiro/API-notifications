using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public class NotificationChannelConfiguration : IEntityTypeConfiguration<NotificationChannel>
{
    public void Configure(EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.ToTable("notification_channels");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.NotificationId)
            .HasColumnName("notification_id")
            .IsRequired();

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(c => c.SentAt)
            .HasColumnName("sent_at");

        builder.HasDiscriminator(c => c.Type)
            .HasValue<EmailChannel>(ChannelType.Email)
            .HasValue<SmsChannel>(ChannelType.Sms)
            .HasValue<PushChannel>(ChannelType.Push);

        builder.HasIndex(c => c.NotificationId)
            .HasDatabaseName("ix_channels_notification_id");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("ix_channels_status");

        builder.HasIndex(c => new { c.NotificationId, c.Type })
            .HasDatabaseName("ix_channels_notification_id_type");
    }
}
