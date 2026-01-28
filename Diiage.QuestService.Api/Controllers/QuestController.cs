using Diiage.QuestService.Api.Models;
using Diiage.QuestService.Api.Models.Requests;
using Diiage.QuestService.Api.Models.Responses;
using Diiage.QuestService.Application.Requests.Commands;
using Diiage.QuestService.Application.Requests.Queries;
using Diiage.QuestService.Domain.Models;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Diiage.QuestService.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des quêtes.
/// </summary>
[ApiController]
public class QuestController(IMediator mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Récupère la liste de toutes les quêtes.
    /// </summary>
    /// <param name="isActive">Filtre optionnel pour récupérer uniquement les quêtes actives ou inactives.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La liste des quêtes.</returns>
    /// <response code="200">Retourne la liste des quêtes.</response>
    [HttpGet(ApiRoutes.Quests.Base)]
    [ProducesResponseType(typeof(IEnumerable<QuestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QuestResponse>>> GetQuests([FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetQuestsQuery { IsActive = isActive }, cancellationToken);

        return Ok(mapper.Map<IEnumerable<Quest>, IEnumerable<QuestResponse>>(response));
    }
    
    /// <summary>
    /// Récupère une quête par son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant de la quête.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La quête correspondante.</returns>
    /// <response code="200">Retourne la quête demandée.</response>
    /// <response code="404">La quête n'a pas été trouvée.</response>
    [HttpGet(ApiRoutes.Quests.ById)]
    [ProducesResponseType(typeof(QuestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestResponse>> GetQuestById([FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetQuestByIdQuery { Id = id }, cancellationToken);

        return Ok(mapper.Map<Quest, QuestResponse>(response));
    }
    
    /// <summary>
    /// Crée une nouvelle quête.
    /// </summary>
    /// <param name="request">Les données de la quête à créer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La quête créée.</returns>
    /// <response code="201">La quête a été créée avec succès.</response>
    /// <response code="400">Les données de la requête sont invalides.</response>
    [HttpPost(ApiRoutes.Quests.Base)]
    [ProducesResponseType(typeof(QuestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuestResponse>> CreateQuest([FromBody] CreateQuestRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(mapper.Map<CreateQuestRequest, CreateQuestCommand>(request),
            cancellationToken);

        return CreatedAtAction(nameof(GetQuestById), new { id = response.Id },
            mapper.Map<Quest, QuestResponse>(response));
    }
    
    /// <summary>
    /// Met à jour une quête existante.
    /// </summary>
    /// <param name="id">L'identifiant de la quête à mettre à jour.</param>
    /// <param name="request">Les nouvelles données de la quête.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La quête mise à jour.</returns>
    /// <response code="200">La quête a été mise à jour avec succès.</response>
    /// <response code="400">Les données de la requête sont invalides.</response>
    /// <response code="404">La quête n'a pas été trouvée.</response>
    [HttpPut(ApiRoutes.Quests.ById)]
    [ProducesResponseType(typeof(QuestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestResponse>> UpdateQuest([FromRoute] int id, [FromBody] UpdateQuestRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = mapper.Map<UpdateQuestRequest, UpdateQuestCommand>(request);
        command.Id = id;
        
        var response = await mediator.Send(command, cancellationToken);

        return Ok(mapper.Map<Quest, QuestResponse>(response));
    }
    
    /// <summary>
    /// Supprime une quête.
    /// </summary>
    /// <param name="id">L'identifiant de la quête à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Aucun contenu.</returns>
    /// <response code="200">La quête a été supprimée avec succès.</response>
    /// <response code="404">La quête n'a pas été trouvée.</response>
    [HttpDelete(ApiRoutes.Quests.ById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestResponse>> DeleteQuest([FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeleteQuestCommand {Id = id},
            cancellationToken);

        return Ok();
    }
}