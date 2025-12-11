using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public class EmailChannelConfiguration : IEntityTypeConfiguration<EmailChannel>
{
    public void Configure(EntityTypeBuilder<EmailChannel> builder)
    {
        builder.Property(e => e.To)
            .HasColumnName("to")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Subject)
            .HasColumnName("subject")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Body)
            .HasColumnName("body")
            .IsRequired()
            .HasColumnType("text");

        builder.Property(e => e.IsBodyHtml)
            .HasColumnName("is_body_html")
            .IsRequired()
            .HasDefaultValue(false);
    }
}
