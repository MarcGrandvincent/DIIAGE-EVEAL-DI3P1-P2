using Diiage.QuestService.Api.Models;
using Diiage.QuestService.Api.Models.Responses;
using Diiage.QuestService.Application.Requests.Queries;
using Diiage.QuestService.Domain.Models;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Diiage.QuestService.Api.Controllers;

[ApiController]
public class PlayerQuestController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet(ApiRoutes.PlayerQuests.Base)]
    
    public async Task<ActionResult<IEnumerable<PlayerQuestResponse>>> GetPlayerQuests([FromRoute] Guid playerId, CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetPlayerQuestsQuery() { PlayerId = playerId }, cancellationToken);
        
        return Ok(mapper.Map<IEnumerable<PlayerQuest>, IEnumerable<PlayerQuestResponse>>(response));
    }
}