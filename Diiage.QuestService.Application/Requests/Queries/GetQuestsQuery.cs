using Diiage.QuestService.Domain.Models;
using MediatR;

namespace Diiage.QuestService.Application.Requests.Queries;

public class GetQuestsQuery : IRequest<IEnumerable<Quest>>
{
    public string? Query { get; set; } = null;
    public bool? IsActive { get; set; } = null;
}