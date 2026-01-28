using Diiage.QuestService.Api.Models;
using Diiage.QuestService.Api.Models.Requests;
using Diiage.QuestService.Domain.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Diiage.QuestService.Api.Controllers;

/// <summary>
/// Contrôleur simulant le GameService pour publier des événements via RabbitMQ.
/// </summary>
[ApiController]
public class GameServiceController(IPublishEndpoint publishEndpoint, ILogger<GameServiceController> logger) : ControllerBase
{
    /// <summary>
    /// Simule la publication d'un événement de complétion (donjon/combat) depuis le GameService.
    /// </summary>
    /// <param name="request">Les données de l'événement.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Confirmation de la publication.</returns>
    /// <response code="202">L'événement a été publié avec succès.</response>
    /// <response code="400">Les données de la requête sont invalides.</response>
    [HttpPost(ApiRoutes.GameService.PublishEvent)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishGameEvent([FromBody] PublishGameEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var gameEvent = new GameCompletedEvent
        {
            EventId = request.EventId,
            PlayerId = request.PlayerId,
            EventType = request.EventType,
            OccurredAt = DateTime.UtcNow,
            Metadata = request.Metadata
        };

        logger.LogInformation(
            "[GameService] Publishing event {EventId} - Player {PlayerId} completed {EventType}",
            gameEvent.EventId,
            gameEvent.PlayerId,
            gameEvent.EventType);

        await publishEndpoint.Publish(gameEvent, cancellationToken);

        logger.LogInformation(
            "[GameService] Event {EventId} published successfully to RabbitMQ",
            gameEvent.EventId);

        return Accepted(new
        {
            Message = "Event published successfully",
            EventId = gameEvent.EventId,
            PublishedAt = gameEvent.OccurredAt
        });
    }
}

