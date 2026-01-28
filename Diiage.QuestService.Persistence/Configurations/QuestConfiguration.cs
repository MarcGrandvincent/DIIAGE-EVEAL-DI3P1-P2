using Diiage.QuestService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diiage.QuestService.Persistence.Configurations;

public class QuestConfiguration : IEntityTypeConfiguration<QuestDao>
{
    public void Configure(EntityTypeBuilder<QuestDao> builder)
    {
        builder.ToTable("Quests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code);
        builder.HasIndex(e => e.Code)
            .IsUnique();
        
        builder.Property(e => e.Title);
        builder.Property(e => e.Description);
        builder.Property(e => e.Type);
        builder.Property(e => e.TargetCount);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.StartAt);
        builder.Property(e => e.EndAt);
        builder.Property(e => e.Reward);
    }
}