namespace Diiage.QuestService.Domain.Events;

/// <summary>
/// Événement publié quand la progression des quêtes a été mise à jour avec succès.
/// </summary>
public record QuestProgressUpdatedEvent
{
    /// <summary>
    /// Identifiant de corrélation (lié à l'EventId original).
    /// </summary>
    public Guid CorrelationId { get; init; }
    
    /// <summary>
    /// Identifiant du joueur.
    /// </summary>
    public Guid PlayerId { get; init; }
    
    /// <summary>
    /// Nombre de quêtes mises à jour.
    /// </summary>
    public int QuestsUpdated { get; init; }
    
    /// <summary>
    /// Nombre de quêtes complétées.
    /// </summary>
    public int QuestsCompleted { get; init; }
    
    /// <summary>
    /// Timestamp du traitement.
    /// </summary>
    public DateTime ProcessedAt { get; init; }
}

