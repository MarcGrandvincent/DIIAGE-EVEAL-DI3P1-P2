using Diiage.QuestService.Application.Sagas;
using Diiage.QuestService.Domain.Enums;
using Diiage.QuestService.Domain.Events;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Diiage.QuestService.Tests.Sagas;

/// <summary>
/// Tests unitaires pour la Saga QuestProgress.
/// Vérifie les transitions d'état et le flux de la transaction distribuée.
/// </summary>
public class QuestProgressSagaTests : IAsyncLifetime
{
    private ITestHarness _harness = null!;
    private ISagaStateMachineTestHarness<QuestProgressSaga, QuestProgressSagaState> _sagaHarness = null!;
    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Mock du logger
        var loggerMock = new Mock<ILogger<QuestProgressSaga>>();
        services.AddSingleton(loggerMock.Object);

        // Configuration MassTransit pour les tests
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<QuestProgressSaga, QuestProgressSagaState>()
                .InMemoryRepository();
        });

        _provider = services.BuildServiceProvider();
        _harness = _provider.GetRequiredService<ITestHarness>();
        _sagaHarness = _provider.GetRequiredService<ISagaStateMachineTestHarness<QuestProgressSaga, QuestProgressSagaState>>();

        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task GameCompletedEvent_ShouldStartSaga_AndTransitionToProcessing()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var gameCompletedEvent = new GameCompletedEvent
        {
            EventId = eventId,
            PlayerId = playerId,
            EventType = DungeonType.DungeonCompletion,
            OccurredAt = DateTime.UtcNow,
            Metadata = "Test dungeon"
        };

        // Act
        await _harness.Bus.Publish(gameCompletedEvent);
        await Task.Delay(200);

        // Assert
        Assert.True(await _harness.Consumed.Any<GameCompletedEvent>());
        
        var saga = _sagaHarness.Sagas.ContainsInState(eventId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Processing);
        Assert.NotNull(saga);

        var instance = _sagaHarness.Sagas.Contains(eventId);
        Assert.NotNull(instance);
        Assert.Equal(playerId, instance!.PlayerId);
        Assert.Equal((int)DungeonType.DungeonCompletion, instance.EventType);
    }

    [Fact]
    public async Task QuestProgressUpdatedEvent_ShouldTransitionToCompleted()
    {
        // Arrange - Démarrer la saga
        var eventId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        
        await _harness.Bus.Publish(new GameCompletedEvent
        {
            EventId = eventId,
            PlayerId = playerId,
            EventType = DungeonType.BossFight,
            OccurredAt = DateTime.UtcNow
        });

        // Attendre que la saga soit en Processing
        await Task.Delay(200);

        // Act - Publier l'événement de succès
        await _harness.Bus.Publish(new QuestProgressUpdatedEvent
        {
            CorrelationId = eventId,
            PlayerId = playerId,
            QuestsUpdated = 2,
            QuestsCompleted = 1,
            ProcessedAt = DateTime.UtcNow
        });

        // Assert
        await Task.Delay(200);
        Assert.True(await _harness.Consumed.Any<QuestProgressUpdatedEvent>());
        
        // La saga devrait être en état Final
        var saga = _sagaHarness.Sagas.Contains(eventId);
        Assert.NotNull(saga);
        Assert.Equal("Final", saga!.CurrentState);
    }

    [Fact]
    public async Task QuestProgressFailedEvent_ShouldTransitionToFailed()
    {
        // Arrange - Démarrer la saga
        var eventId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        
        await _harness.Bus.Publish(new GameCompletedEvent
        {
            EventId = eventId,
            PlayerId = playerId,
            EventType = DungeonType.Puzzle,
            OccurredAt = DateTime.UtcNow
        });

        // Attendre que la saga soit en Processing
        await Task.Delay(200);

        // Act - Publier l'événement d'échec
        await _harness.Bus.Publish(new QuestProgressFailedEvent
        {
            CorrelationId = eventId,
            PlayerId = playerId,
            Reason = "Database connection failed",
            FailedAt = DateTime.UtcNow
        });

        // Assert
        await Task.Delay(200);
        Assert.True(await _harness.Consumed.Any<QuestProgressFailedEvent>());
        
        // La saga devrait être en état Final avec la raison de l'échec
        var saga = _sagaHarness.Sagas.Contains(eventId);
        Assert.NotNull(saga);
        Assert.Equal("Final", saga!.CurrentState);
        Assert.Equal("Database connection failed", saga.FailureReason);
    }

    [Fact]
    public async Task MultipleSagas_ShouldBeIndependent()
    {
        // Arrange
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();

        // Act - Démarrer deux sagas
        await _harness.Bus.Publish(new GameCompletedEvent
        {
            EventId = eventId1,
            PlayerId = playerId1,
            EventType = DungeonType.DungeonCompletion,
            OccurredAt = DateTime.UtcNow
        });

        await _harness.Bus.Publish(new GameCompletedEvent
        {
            EventId = eventId2,
            PlayerId = playerId2,
            EventType = DungeonType.BossFight,
            OccurredAt = DateTime.UtcNow
        });

        await Task.Delay(200);

        // Compléter seulement la première saga
        await _harness.Bus.Publish(new QuestProgressUpdatedEvent
        {
            CorrelationId = eventId1,
            PlayerId = playerId1,
            QuestsUpdated = 1,
            QuestsCompleted = 0,
            ProcessedAt = DateTime.UtcNow
        });

        await Task.Delay(200);

        // Assert
        var saga1 = _sagaHarness.Sagas.Contains(eventId1);
        Assert.NotNull(saga1);
        Assert.Equal("Final", saga1!.CurrentState); // Saga 1 finalisée

        var saga2 = _sagaHarness.Sagas.Contains(eventId2);
        Assert.NotNull(saga2);
        Assert.Equal("Processing", saga2!.CurrentState); // Saga 2 toujours en Processing
    }
}

