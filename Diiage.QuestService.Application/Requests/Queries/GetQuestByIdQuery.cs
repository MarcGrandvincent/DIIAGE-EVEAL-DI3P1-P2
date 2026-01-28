using Diiage.QuestService.Domain.Models;
using MediatR;

namespace Diiage.QuestService.Application.Requests.Queries;

public class GetQuestByIdQuery : IRequest<Quest>
{
    public int Id { get; set; }
}