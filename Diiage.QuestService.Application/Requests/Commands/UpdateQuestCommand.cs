using Diiage.QuestService.Domain.Enums;
using Diiage.QuestService.Domain.Models;
using MediatR;

namespace Diiage.QuestService.Application.Requests.Commands;

public class UpdateQuestCommand : IRequest<Quest>
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DungeonType Type { get; set; }
    public int TargetCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Reward { get; set; } = string.Empty;
}

