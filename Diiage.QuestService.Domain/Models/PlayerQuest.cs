using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Enums;

namespace Diiage.QuestService.Domain.Models;

public class PlayerQuest
{
    public Guid PlayerId { get; set; }
    public int QuestId { get; set; }
    public QuestStatus Status { get; set; }
    public int ProgressCount { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Quest Quest { get; set; }
}