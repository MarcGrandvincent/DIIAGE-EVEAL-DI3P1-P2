using Diiage.QuestService.Domain.Models;
using MediatR;

namespace Diiage.QuestService.Application.Requests.Queries;

public class GetPlayerQuestsQuery : IRequest<IEnumerable<PlayerQuest>>
{
    public Guid PlayerId { get; set; }
}