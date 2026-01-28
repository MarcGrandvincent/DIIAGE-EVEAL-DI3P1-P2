using Diiage.QuestService.Application.Consumers;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Enums;
using Diiage.QuestService.Domain.Events;
using Diiage.QuestService.Persistence;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Diiage.QuestService.Tests.Consumers;

/// <summary>
/// Tests unitaires pour le Consumer GameCompleted.
/// Vérifie l'idempotence, la mise à jour des quêtes et la publication des événements.
/// </summary>
public class GameCompletedConsumerTests : IAsyncLifetime
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private DbContextCore _dbContext = null!;
    private string _dbName = null!;

    public async Task InitializeAsync()
    {
        _dbName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();

        // Configuration DbContext InMemory avec le même nom pour partager les données
        services.AddDbContext<DbContextCore>(options =>
                options.UseInMemoryDatabase(_dbName)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)),
            ServiceLifetime.Singleton, ServiceLifetime.Singleton);

        // Mock du logger
        var loggerMock = new Mock<ILogger<GameCompletedConsumer>>();
        services.AddSingleton(loggerMock.Object);

        // Configuration MassTransit pour les tests
        services.AddMassTransitTestHarness(cfg => { cfg.AddConsumer<GameCompletedConsumer>(); });

        _provider = services.BuildServiceProvider();
        _harness = _provider.GetRequiredService<ITestHarness>();
        _dbContext = _provider.GetRequiredService<DbContextCore>();

        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Consume_ShouldUpdateQuestProgress_WhenMatchingQuestExists()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var quest = new QuestDao
        {
            Id = 1,
            Code = "QUEST_001",
            Title = "Complete 3 Dungeons",
            Description = "Test quest",
            Type = DungeonType.DungeonCompletion,
            TargetCount = 3,
            IsActive = true,
            StartAt = DateTime.UtcNow.AddDays(-1),
            EndAt = DateTime.UtcNow.AddDays(30),
            Reward = "100 Gold"
        };

        var playerQuest = new PlayerQuestDao
        {
            PlayerId = playerId,
            QuestId = 1,
            Status = QuestStatus.NotStarted,
            ProgressCount = 0,
            Quest = quest
        };

        _dbContext.Set<QuestDao>().Add(quest);
        _dbContext.Set<PlayerQuestDao>().Add(playerQuest);
        await _dbContext.SaveChangesAsync();

        var eventId = Guid.NewGuid();
        var gameCompletedEvent = new GameCompletedEvent
        {
            EventId = eventId,
            PlayerId = playerId,
            EventType = DungeonType.DungeonCompletion,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await _harness.Bus.Publish(gameCompletedEvent);
        await Task.Delay(500);

        // Assert
        Assert.True(await _harness.Consumed.Any<GameCompletedEvent>());
        Assert.True(await _harness.Published.Any<QuestProgressUpdatedEvent>());

        var updatedPlayerQuest = await _dbContext.Set<PlayerQuestDao>()
            .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == 1);

        Assert.NotNull(updatedPlayerQuest);
        Assert.Equal(1, updatedPlayerQuest!.ProgressCount);
        Assert.Equal(QuestStatus.InProgress, updatedPlayerQuest.Status);
    }

    [Fact]
    public async Task Consume_ShouldCompleteQuest_WhenTargetReached()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var quest = new QuestDao
        {
            Id = 2,
            Code = "QUEST_002",
            Title = "Kill 1 Boss",
            Description = "Test quest",
            Type = DungeonType.BossFight,
            TargetCount = 1, // Seulement 1 requis
            IsActive = true,
            StartAt = DateTime.UtcNow.AddDays(-1),
            EndAt = DateTime.UtcNow.AddDays(30),
            Reward = "Epic Sword"
        };

        var playerQuest = new PlayerQuestDao
        {
            PlayerId = playerId,
            QuestId = 2,
            Status = QuestStatus.NotStarted,
            ProgressCount = 0,
            Quest = quest
        };

        _dbContext.Set<QuestDao>().Add(quest);
        _dbContext.Set<PlayerQuestDao>().Add(playerQuest);
        await _dbContext.SaveChangesAsync();

        var eventId = Guid.NewGuid();
        var gameCompletedEvent = new GameCompletedEvent
        {
            EventId = eventId,
            PlayerId = playerId,
            EventType = DungeonType.BossFight,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await _harness.Bus.Publish(gameCompletedEvent);
        await Task.Delay(500);

        // Assert
        var updatedPlayerQuest = await _dbContext.Set<PlayerQuestDao>()
            .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == 2);

        Assert.NotNull(updatedPlayerQuest);
        Assert.Equal(QuestStatus.Completed, updatedPlayerQuest!.Status);
    }

    [Fact]
    public async Task Consume_ShouldBeIdempotent_WhenSameEventProcessedTwice()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var quest = new QuestDao
        {
            Id = 3,
            Code = "QUEST_003",
            Title = "Explore 5 Areas",
            Description = "Test quest",
            Type = DungeonType.Exploration,
            TargetCount = 5,
            IsActive = true,
            StartAt = DateTime.UtcNow.AddDays(-1),
            EndAt = DateTime.UtcNow.AddDays(30),
            Reward = "Map"
        };

        var playerQuest = new PlayerQuestDao
        {
            PlayerId = playerId,
            QuestId = 3,
            Status = QuestStatus.NotStarted,
            ProgressCount = 0,
            Quest = quest
        };

        _dbContext.Set<QuestDao>().Add(quest);
        _dbContext.Set<PlayerQuestDao>().Add(playerQuest);
        await _dbContext.SaveChangesAsync();

        var eventId = Guid.NewGuid(); // Même EventId pour les deux
        var gameCompletedEvent = new GameCompletedEvent
        {
            EventId = eventId,
            PlayerId = playerId,
            EventType = DungeonType.Exploration,
            OccurredAt = DateTime.UtcNow
        };

        // Act - Envoyer le même événement deux fois
        await _harness.Bus.Publish(gameCompletedEvent);
        await Task.Delay(500);
        await _harness.Bus.Publish(gameCompletedEvent); // Doublon
        await Task.Delay(500);

        // Assert - Le progrès ne devrait être incrémenté qu'une seule fois (idempotence)
        var updatedPlayerQuest = await _dbContext.Set<PlayerQuestDao>()
            .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == 3);

        Assert.NotNull(updatedPlayerQuest);
        Assert.Equal(1, updatedPlayerQuest!.ProgressCount); // Idempotence

        // Vérifier qu'il n'y a qu'une seule entrée dans l'Inbox
        var inboxCount = await _dbContext.InboxStates.CountAsync(x => x.EventId == eventId);
        Assert.Equal(1, inboxCount);
    }

    [Fact]
    public async Task Consume_ShouldPublishSuccessEvent_WhenNoMatchingQuest()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        // Pas de quête créée pour ce joueur
        var gameCompletedEvent = new GameCompletedEvent
        {
            EventId = eventId,
            PlayerId = playerId,
            EventType = DungeonType.DungeonCompletion,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await _harness.Bus.Publish(gameCompletedEvent);
        await Task.Delay(500);

        // Assert
        Assert.True(await _harness.Consumed.Any<GameCompletedEvent>());
        Assert.True(await _harness.Published.Any<QuestProgressUpdatedEvent>());
    }
}

