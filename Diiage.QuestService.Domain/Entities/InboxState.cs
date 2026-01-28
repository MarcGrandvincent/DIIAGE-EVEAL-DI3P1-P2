namespace Diiage.QuestService.Domain.Entities;

/// <summary>
/// Table Inbox pour garantir l'idempotence des messages consommés.
/// Chaque EventId traité est enregistré pour éviter le double traitement.
/// </summary>
public class InboxState
{
    /// <summary>
    /// Identifiant unique de l'événement (clé primaire).
    /// </summary>
    public Guid EventId { get; set; }
    
    /// <summary>
    /// Type de l'événement (ex: "GameCompletedEvent").
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Date de réception et traitement de l'événement.
    /// </summary>
    public DateTime ProcessedAt { get; set; }
    
    /// <summary>
    /// Identifiant du consumer qui a traité l'événement.
    /// </summary>
    public string ConsumerType { get; set; } = string.Empty;
}

