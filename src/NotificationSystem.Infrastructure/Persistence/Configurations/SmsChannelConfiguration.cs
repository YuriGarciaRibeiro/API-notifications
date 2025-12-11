using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public class SmsChannelConfiguration : IEntityTypeConfiguration<SmsChannel>
{
    public void Configure(EntityTypeBuilder<SmsChannel> builder)
    {
        builder.Property(s => s.To)
            .HasColumnName("to")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(1600);

        builder.Property(s => s.SenderId)
            .HasColumnName("sender_id")
            .HasMaxLength(50);
    }
}
