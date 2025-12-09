using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public class SmsNotificationConfiguration : IEntityTypeConfiguration<SmsNotification>
{
    public void Configure(EntityTypeBuilder<SmsNotification> builder)
    {
        builder.Property(s => s.To)
            .HasColumnName("to")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(1600);

        builder.Property(s => s.SenderId)
            .HasColumnName("sender_id")
            .HasMaxLength(50);
    }
}
