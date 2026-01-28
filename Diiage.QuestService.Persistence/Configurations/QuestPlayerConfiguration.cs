using Diiage.QuestService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diiage.QuestService.Persistence.Configurations;

public class QuestPlayerConfiguration : IEntityTypeConfiguration<PlayerQuestDao>
{
    public void Configure(EntityTypeBuilder<PlayerQuestDao> builder)
    {
        builder.ToTable("PlayerQuests");

        builder.HasKey(e => e.PlayerId);

        builder.Property(e => e.Status);
        builder.Property(e => e.ProgressCount);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CompletedAt);
        
        builder.HasOne(e => e.Quest)
            .WithMany(e => e.PlayerQuests)
            .HasForeignKey(e => e.QuestId);
    }
}