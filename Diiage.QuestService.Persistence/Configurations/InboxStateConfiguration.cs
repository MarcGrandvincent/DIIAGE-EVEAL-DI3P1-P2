using Diiage.QuestService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diiage.QuestService.Persistence.Configurations;

/// <summary>
/// Configuration EF Core pour la table InboxState (idempotence).
/// </summary>
public class InboxStateConfiguration : IEntityTypeConfiguration<InboxState>
{
    public void Configure(EntityTypeBuilder<InboxState> builder)
    {
        builder.ToTable("InboxStates");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(e => e.ConsumerType)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(e => e.ProcessedAt)
            .IsRequired();

        // Index pour recherche rapide par EventId + ConsumerType
        builder.HasIndex(e => new { e.EventId, e.ConsumerType })
            .IsUnique();
    }
}

