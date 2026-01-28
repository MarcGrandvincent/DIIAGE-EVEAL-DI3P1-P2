using Diiage.QuestService.Domain.Enums;

namespace Diiage.QuestService.Domain.Entities;

public class PlayerQuestDao
{
    public Guid PlayerId { get; set; }
    
    public int QuestId { get; set; }
    public QuestStatus Status { get; set; }
    public int ProgressCount { get; set; }
    public DateTime CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public QuestDao Quest { get; set; }
}