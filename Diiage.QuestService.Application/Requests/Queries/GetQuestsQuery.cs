using Diiage.QuestService.Domain.Models;
using MediatR;

namespace Diiage.QuestService.Application.Requests.Queries;

public class GetQuestsQuery : IRequest<IEnumerable<Quest>>
{
    public bool? IsActive { get; set; } = null;
}