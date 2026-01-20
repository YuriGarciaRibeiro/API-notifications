using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public class ProviderConfigurationConfiguration : IEntityTypeConfiguration<ProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<ProviderConfiguration> builder)
    {
        builder.ToTable("provider_configurations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.ChannelType)
            .HasColumnName("channel_type")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Provider)
            .HasColumnName("provider")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.ConfigurationJson)
            .HasColumnName("configuration_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.isPrimary)
            .HasColumnName("is_primary")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.Priority)
            .HasColumnName("priority")
            .IsRequired()
            .HasDefaultValue(0);

        // Índice único: apenas um provedor primário por canal
        builder.HasIndex(p => new { p.ChannelType, p.isPrimary })
            .HasDatabaseName("ix_provider_configurations_channel_type_is_primary")
            .IsUnique()
            .HasFilter("is_primary = true");

        // Índice para busca por canal
        builder.HasIndex(p => p.ChannelType)
            .HasDatabaseName("ix_provider_configurations_channel_type");

        // Índice para busca por provedores ativos
        builder.HasIndex(p => new { p.ChannelType, p.IsActive })
            .HasDatabaseName("ix_provider_configurations_channel_type_is_active");
    }
}
