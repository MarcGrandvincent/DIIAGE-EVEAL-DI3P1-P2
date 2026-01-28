namespace Diiage.QuestService.Domain.Events;

/// <summary>
/// Événement publié quand le traitement de la progression a échoué.
/// Permet au GameService de déclencher une compensation.
/// </summary>
public record QuestProgressFailedEvent
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
    /// Raison de l'échec.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
    
    /// <summary>
    /// Timestamp de l'échec.
    /// </summary>
    public DateTime FailedAt { get; init; }
}

