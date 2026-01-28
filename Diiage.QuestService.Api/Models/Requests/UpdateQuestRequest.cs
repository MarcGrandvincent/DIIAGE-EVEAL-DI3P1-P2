using Diiage.QuestService.Domain.Enums;

namespace Diiage.QuestService.Api.Models.Requests;

public class UpdateQuestRequest
{
    public string Reward { get; set; } = string.Empty;
    public DateTime EndAt { get; set; }
    public DateTime StartAt { get; set; }
    public bool IsActive { get; set; }
    public int TargetCount { get; set; }
    public DungeonType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}