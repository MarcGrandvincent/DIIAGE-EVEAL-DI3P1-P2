using MediatR;

namespace Diiage.QuestService.Application.Requests.Commands;

public class DeleteQuestCommand : IRequest
{
    public int Id { get; set; }
}