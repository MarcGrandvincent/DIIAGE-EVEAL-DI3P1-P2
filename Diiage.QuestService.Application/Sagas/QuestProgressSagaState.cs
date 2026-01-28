using MassTransit;

namespace Diiage.QuestService.Application.Sagas;

/// <summary>
/// État persisté de la Saga QuestProgress.
/// </summary>
public class QuestProgressSagaState : SagaStateMachineInstance
{
    /// <summary>
    /// Identifiant de corrélation (= EventId du GameCompletedEvent).
    /// </summary>
    public Guid CorrelationId { get; set; }
    
    /// <summary>
    /// État actuel de la saga.
    /// </summary>
    public string CurrentState { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifiant du joueur.
    /// </summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>
    /// Type d'événement traité.
    /// </summary>
    public int EventType { get; set; }
    
    /// <summary>
    /// Date de création de la saga.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date de dernière mise à jour.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Nombre de quêtes mises à jour (rempli après succès).
    /// </summary>
    public int QuestsUpdated { get; set; }
    
    /// <summary>
    /// Nombre de quêtes complétées (rempli après succès).
    /// </summary>
    public int QuestsCompleted { get; set; }
    
    /// <summary>
    /// Raison de l'échec (si applicable).
    /// </summary>
    public string? FailureReason { get; set; }
}

