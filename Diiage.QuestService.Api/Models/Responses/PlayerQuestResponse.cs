using Diiage.QuestService.Domain.Enums;

namespace Diiage.QuestService.Api.Models.Responses;

public class PlayerQuestResponse
{
    public Guid PlayerId { get; set; }
    public int QuestId { get; set; }
    public QuestStatus Status { get; set; }
    public int ProgressCount { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public QuestResponse Quest { get; set; }
}