using Diiage.QuestService.Domain.Enums;

namespace Diiage.QuestService.Api.Models.Requests;

/// <summary>
/// Requête pour simuler un événement provenant du GameService.
/// </summary>
public class PublishGameEventRequest
{
    /// <summary>
    /// Identifiant unique du joueur.
    /// </summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>
    /// Type d'événement (DungeonCompletion, BossFight, etc.).
    /// </summary>
    public DungeonType EventType { get; set; }
    
    /// <summary>
    /// Identifiant unique de l'événement pour l'idempotence.
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Données additionnelles (ex: nom du donjon, niveau du boss).
    /// </summary>
    public string? Metadata { get; set; }
}

