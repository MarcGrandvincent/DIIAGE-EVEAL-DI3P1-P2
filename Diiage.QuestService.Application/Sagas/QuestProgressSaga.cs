using Diiage.QuestService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Diiage.QuestService.Application.Sagas;

/// <summary>
/// State Machine Saga pour orchestrer le flux de progression des quêtes.
/// </summary>
public class QuestProgressSaga : MassTransitStateMachine<QuestProgressSagaState>
{
    private readonly ILogger<QuestProgressSaga> _logger;

    public QuestProgressSaga(ILogger<QuestProgressSaga> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Event(() => GameCompleted, x => x.CorrelateById(ctx => ctx.Message.EventId));
        Event(() => ProgressUpdated, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => ProgressFailed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Initially(
            When(GameCompleted)
                .Then(ctx =>
                {
                    ctx.Saga.PlayerId = ctx.Message.PlayerId;
                    ctx.Saga.EventType = (int)ctx.Message.EventType;
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "[Saga] Started - CorrelationId: {CorrelationId}, Player: {PlayerId}, EventType: {EventType}",
                        ctx.Saga.CorrelationId,
                        ctx.Saga.PlayerId,
                        ctx.Message.EventType);
                })
                .TransitionTo(Processing)
        );

        During(Processing,
            When(ProgressUpdated)
                .Then(ctx =>
                {
                    ctx.Saga.QuestsUpdated = ctx.Message.QuestsUpdated;
                    ctx.Saga.QuestsCompleted = ctx.Message.QuestsCompleted;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "[Saga] Completed successfully - CorrelationId: {CorrelationId}, QuestsUpdated: {QuestsUpdated}, QuestsCompleted: {QuestsCompleted}",
                        ctx.Saga.CorrelationId,
                        ctx.Saga.QuestsUpdated,
                        ctx.Saga.QuestsCompleted);
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(ProgressFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;

                    _logger.LogError(
                        "[Saga] Failed - CorrelationId: {CorrelationId}, Reason: {Reason}. Compensation may be required.",
                        ctx.Saga.CorrelationId,
                        ctx.Saga.FailureReason);
                })
                .TransitionTo(Failed)
                .Finalize()
        );

        // Supprimer les sagas finalisées
        SetCompletedWhenFinalized();
    }

    // États de la saga
    public State Processing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // Événements
    public Event<GameCompletedEvent> GameCompleted { get; private set; } = null!;
    public Event<QuestProgressUpdatedEvent> ProgressUpdated { get; private set; } = null!;
    public Event<QuestProgressFailedEvent> ProgressFailed { get; private set; } = null!;
}

