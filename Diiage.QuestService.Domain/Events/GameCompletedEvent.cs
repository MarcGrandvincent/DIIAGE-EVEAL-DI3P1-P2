using Diiage.QuestService.Domain.Enums;

namespace Diiage.QuestService.Domain.Events;

/// <summary>
/// Événement publié quand un joueur complète un donjon/combat dans le GameService.
/// </summary>
public record GameCompletedEvent
{
    /// <summary>
    /// Identifiant unique de l'événement (pour idempotence).
    /// </summary>
    public Guid EventId { get; init; }
    
    /// <summary>
    /// Identifiant du joueur.
    /// </summary>
    public Guid PlayerId { get; init; }
    
    /// <summary>
    /// Type d'action complétée.
    /// </summary>
    public DungeonType EventType { get; init; }
    
    /// <summary>
    /// Timestamp de l'événement.
    /// </summary>
    public DateTime OccurredAt { get; init; }
    
    /// <summary>
    /// Métadonnées additionnelles.
    /// </summary>
    public string? Metadata { get; init; }
}

