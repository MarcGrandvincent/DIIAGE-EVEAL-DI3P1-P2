using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Enums;
using Diiage.QuestService.Domain.Events;
using Diiage.QuestService.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Diiage.QuestService.Application.Consumers;

/// <summary>
/// Consumer MassTransit pour traiter les événements GameCompleted.
/// Implémente le pattern Inbox pour garantir l'idempotence.
/// Publie des événements de réponse pour la Saga.
/// </summary>
public class GameCompletedConsumer(DbContextCore dbContext, ILogger<GameCompletedConsumer> logger) 
    : IConsumer<GameCompletedEvent>
{
    public async Task Consume(ConsumeContext<GameCompletedEvent> context)
    {
        var message = context.Message;
        const string consumerType = nameof(GameCompletedConsumer);

        logger.LogInformation(
            "[Consumer] Received event {EventId} - Player {PlayerId} completed {EventType}",
            message.EventId,
            message.PlayerId,
            message.EventType);

        // === IDEMPOTENCE CHECK (Inbox Pattern) ===
        var alreadyProcessed = await dbContext.InboxStates
            .AnyAsync(x => x.EventId == message.EventId && x.ConsumerType == consumerType);

        if (alreadyProcessed)
        {
            logger.LogWarning(
                "[Consumer] Event {EventId} already processed - skipping (idempotence)",
                message.EventId);
            return;
        }

        // === START TRANSACTION ===
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var inboxEntry = new InboxState
            {
                EventId = message.EventId,
                EventType = nameof(GameCompletedEvent),
                ConsumerType = consumerType,
                ProcessedAt = DateTime.UtcNow
            };
            dbContext.InboxStates.Add(inboxEntry);
            
            var questsUpdated = 0;
            var questsCompleted = 0;
            (questsUpdated, questsCompleted) = await UpdatePlayerQuestProgress(message);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation(
                "[Consumer] Event {EventId} processed successfully - Player {PlayerId} quest progress updated",
                message.EventId,
                message.PlayerId);
            
            await context.Publish(new QuestProgressUpdatedEvent
            {
                CorrelationId = message.EventId,
                PlayerId = message.PlayerId,
                QuestsUpdated = questsUpdated,
                QuestsCompleted = questsCompleted,
                ProcessedAt = DateTime.UtcNow
            });

            logger.LogInformation(
                "[Consumer] Published QuestProgressUpdatedEvent for CorrelationId {CorrelationId}",
                message.EventId);
        }
        catch (Exception ex)
        {
            // === COMPENSATION / ROLLBACK ===
            await transaction.RollbackAsync();

            logger.LogError(ex,
                "[Consumer] Error processing event {EventId} - Transaction rolled back (compensation)",
                message.EventId);

            // Publier l'événement d'échec pour la Saga (compensation côté GameService)
            await context.Publish(new QuestProgressFailedEvent
            {
                CorrelationId = message.EventId,
                PlayerId = message.PlayerId,
                Reason = ex.Message,
                FailedAt = DateTime.UtcNow
            });

            logger.LogInformation(
                "[Consumer] Published QuestProgressFailedEvent for CorrelationId {CorrelationId}",
                message.EventId);

            throw; // Re-throw pour que MassTransit gère le retry
        }
    }

    /// <summary>
    /// Met à jour la progression des quêtes actives du joueur correspondant au type d'événement.
    /// </summary>
    /// <returns>Tuple (questsUpdated, questsCompleted)</returns>
    private async Task<(int questsUpdated, int questsCompleted)> UpdatePlayerQuestProgress(GameCompletedEvent message)
    {
        var questsUpdated = 0;
        var questsCompleted = 0;

        // Récupérer les quêtes actives du joueur qui correspondent au type d'événement
        var playerQuests = await dbContext.Set<PlayerQuestDao>()
            .Include(pq => pq.Quest)
            .Where(pq => pq.PlayerId == message.PlayerId
                         && pq.Quest.Type == message.EventType
                         && pq.Quest.IsActive
                         && pq.Status != QuestStatus.Completed
                         && pq.Status != QuestStatus.Claimed)
            .ToListAsync();

        if (playerQuests.Count == 0)
        {
            logger.LogInformation(
                "[Consumer] No matching active quests found for Player {PlayerId} with type {EventType}",
                message.PlayerId,
                message.EventType);
            return (0, 0);
        }

        foreach (var playerQuest in playerQuests)
        {
            playerQuest.ProgressCount++;
            playerQuest.UpdatedAt = DateTime.UtcNow;
            questsUpdated++;

            // Vérifier si la quête est complétée
            if (playerQuest.ProgressCount >= playerQuest.Quest.TargetCount)
            {
                playerQuest.Status = QuestStatus.Completed;
                playerQuest.CompletedAt = DateTime.UtcNow;
                questsCompleted++;

                logger.LogInformation(
                    "[Consumer] Quest {QuestId} completed for Player {PlayerId}!",
                    playerQuest.QuestId,
                    playerQuest.PlayerId);
            }
            else
            {
                playerQuest.Status = QuestStatus.InProgress;

                logger.LogInformation(
                    "[Consumer] Quest {QuestId} progress updated for Player {PlayerId}: {Progress}/{Target}",
                    playerQuest.QuestId,
                    playerQuest.PlayerId,
                    playerQuest.ProgressCount,
                    playerQuest.Quest.TargetCount);
            }
        }

        return (questsUpdated, questsCompleted);
    }
}

