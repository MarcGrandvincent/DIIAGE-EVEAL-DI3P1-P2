# Transaction Distribuée - Saga MassTransit + RabbitMQ

## Vue d'ensemble

Ce projet implémente une **transaction distribuée** entre un microservice Game (simulé) et le microservice Quest, utilisant le pattern **Saga** avec **MassTransit** et **RabbitMQ**.

## Architecture

```
┌─────────────────────┐         RabbitMQ          ┌─────────────────────┐
│    GameService      │    ─────────────────►     │    QuestService     │
│  (endpoint simulé)  │    GameCompletedEvent     │     (consumer)      │
└─────────────────────┘                           └─────────────────────┘
         │                                                  │
         │                                                  │
         ▼                                                  ▼
   POST /api/v1/                                   ┌───────────────┐
   gameservice/                                    │  InboxState   │
   publish-event                                   │ (idempotence) │
                                                   └───────────────┘
                                                           │
                                                           ▼
                                                   ┌───────────────┐
                                                   │ PlayerQuests  │
                                                   │  (progress)   │
                                                   └───────────────┘
```

---

## Saga State Machine

### Diagramme d'états

```
                    GameCompletedEvent
                           │
                           ▼
                    ┌─────────────┐
                    │   Initial   │
                    └─────────────┘
                           │
                           ▼
                    ┌─────────────┐
                    │  Processing │
                    └─────────────┘
                      /         \
    QuestProgressUpdated    QuestProgressFailed
                    /             \
                   ▼               ▼
          ┌───────────┐     ┌─────────┐
          │ Completed │     │ Failed  │
          └───────────┘     └─────────┘
                                 │
                                 ▼
                          Compensation
                          (GameService)
```

### États de la Saga

| État | Description |
|------|-------------|
| `Initial` | État initial, en attente d'un `GameCompletedEvent` |
| `Processing` | Traitement en cours par le consumer |
| `Completed` | Traitement réussi, quêtes mises à jour |
| `Failed` | Échec du traitement, compensation requise |

### Événements

| Événement | Direction | Description |
|-----------|-----------|-------------|
| `GameCompletedEvent` | GameService → QuestService | Déclenche le traitement |
| `QuestProgressUpdatedEvent` | QuestService → Saga | Succès du traitement |
| `QuestProgressFailedEvent` | QuestService → Saga/GameService | Échec, déclenche compensation |

---

## Flux détaillé

### 1. Publication de l'événement (GameService simulé)

**Fichiers :**
- `Controllers/GameServiceController.cs`
- `Models/Requests/PublishGameEventRequest.cs`
- `Domain/Events/GameCompletedEvent.cs`

**Endpoint :** `POST /api/v1/gameservice/publish-event`

**Payload :**
```json
{
  "playerId": "guid-du-joueur",
  "eventType": 0,  // 0=DungeonCompletion, 1=BossFight, 2=Puzzle, 3=Exploration
  "eventId": "guid-unique-pour-idempotence",
  "metadata": "informations optionnelles"
}
```

**Action :** Publie un `GameCompletedEvent` sur RabbitMQ via MassTransit.

---

### 2. Saga démarre

**Fichiers :**
- `Application/Sagas/QuestProgressSaga.cs`
- `Application/Sagas/QuestProgressSagaState.cs`

**Action :**
1. La Saga reçoit le `GameCompletedEvent`
2. Crée une instance de `QuestProgressSagaState`
3. Passe à l'état `Processing`
4. Log le démarrage

---

### 3. Consommation de l'événement (Consumer)

**Fichier :** `Application/Consumers/GameCompletedConsumer.cs`

**Étapes :**

1. **Réception** du message depuis RabbitMQ
2. **Vérification idempotence** (Inbox Pattern) - vérifie si `EventId` existe dans `InboxStates`
3. **Si déjà traité** → log warning et skip
4. **Si nouveau** → démarre une transaction :
   - Insère dans `InboxStates` (marque comme traité)
   - Met à jour la progression des quêtes du joueur
   - Commit la transaction
5. **Si succès** → Publie `QuestProgressUpdatedEvent`
6. **Si erreur** → Rollback + Publie `QuestProgressFailedEvent`

---

### 4. Saga se finalise

**Si succès (`QuestProgressUpdatedEvent`) :**
- Saga passe à l'état `Completed`
- Log le succès avec le nombre de quêtes mises à jour
- Saga finalisée et supprimée

**Si échec (`QuestProgressFailedEvent`) :**
- Saga passe à l'état `Failed`
- Log l'erreur avec la raison
- **Le GameService peut consommer cet événement pour compenser** (annuler la complétion côté jeu)
- Saga finalisée

---

## Pattern Inbox (Idempotence)

**Fichiers :**
- `Persistence/Entities/InboxState.cs`
- `Persistence/Configurations/InboxStateConfiguration.cs`

**Table `InboxStates` :**
| Colonne | Type | Description |
|---------|------|-------------|
| EventId | GUID | ID unique de l'événement (PK) |
| EventType | string | Type de l'événement |
| ConsumerType | string | Nom du consumer |
| ProcessedAt | DateTime | Date de traitement |

**Garantie :** Un même événement ne sera jamais traité deux fois, même en cas de re-delivery RabbitMQ.

---

## Garanties implémentées

| Exigence | Implémentation |
|----------|----------------|
| **At-least-once delivery** | MassTransit + RabbitMQ (ack après traitement) |
| **Idempotence consumer** | Pattern Inbox avec table `InboxStates` |
| **Traces/logs** | ILogger avec préfixes `[GameService]`, `[Consumer]`, `[Saga]` |
| **Compensation en erreur** | Transaction DB avec rollback + `QuestProgressFailedEvent` |
| **Orchestration** | Saga State Machine MassTransit |

---

## Configuration RabbitMQ

**Fichier :** `Configurations/Installers/RabbitMqInstaller.cs`

**appsettings.json :**
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": "5672"
  }
}
```

---

## Test du flux

1. **Démarrer RabbitMQ** (Docker ou local)
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
   ```

2. **Lancer l'API**

3. **Envoyer un événement :**
   ```bash
   curl -X POST https://localhost:7xxx/api/v1/gameservice/publish-event \
     -H "Content-Type: application/json" \
     -d '{
       "playerId": "11111111-1111-1111-1111-111111111111",
       "eventType": 0,
       "metadata": "Donjon niveau 5"
     }'
   ```

4. **Observer les logs** pour voir le flux complet :
   ```
   [GameService] Publishing event xxx - Player xxx completed DungeonCompletion
   [GameService] Event xxx published successfully to RabbitMQ
   [Saga] Started - CorrelationId: xxx, Player: xxx, EventType: DungeonCompletion
   [Consumer] Received event xxx - Player xxx completed DungeonCompletion
   [Consumer] Quest 1 progress updated for Player xxx: 1/5
   [Consumer] Event xxx processed successfully
   [Consumer] Published QuestProgressUpdatedEvent for CorrelationId xxx
   [Saga] Completed successfully - CorrelationId: xxx, QuestsUpdated: 1, QuestsCompleted: 0
   ```

5. **Renvoyer le même eventId** pour vérifier l'idempotence

---

## Structure des fichiers

```
Diiage.QuestService.Api/
├── Controllers/
│   └── GameServiceController.cs      # Endpoint simulant GameService
├── Models/Requests/
│   └── PublishGameEventRequest.cs    # DTO de la requête
└── Configurations/Installers/
    └── RabbitMqInstaller.cs          # Config MassTransit + Consumer + Saga

Diiage.QuestService.Domain/
└── Events/
    ├── GameCompletedEvent.cs         # Message initial
    ├── QuestProgressUpdatedEvent.cs  # Event succès
    └── QuestProgressFailedEvent.cs   # Event échec (compensation)

Diiage.QuestService.Application/
├── Consumers/
│   └── GameCompletedConsumer.cs      # Consumer avec Inbox pattern
└── Sagas/
    ├── QuestProgressSaga.cs          # State Machine
    └── QuestProgressSagaState.cs     # État persisté

Diiage.QuestService.Persistence/
├── Entities/
│   └── InboxState.cs                 # Entité Inbox
├── Configurations/
│   └── InboxStateConfiguration.cs    # Config EF Core
└── DbContextCore.cs                  # DbSet InboxStates

Diiage.QuestService.Tests/
├── Sagas/
│   └── QuestProgressSagaTests.cs     # Tests de la Saga
└── Consumers/
    └── GameCompletedConsumerTests.cs # Tests du Consumer
```

---

## Tests Unitaires

### Exécution des tests

```bash
cd Diiage.QuestService.Tests
dotnet test
```

### Tests de la Saga (`QuestProgressSagaTests.cs`)

| Test | Description |
|------|-------------|
| `GameCompletedEvent_ShouldStartSaga_AndTransitionToProcessing` | Vérifie que la saga démarre et passe en état Processing |
| `QuestProgressUpdatedEvent_ShouldTransitionToCompleted` | Vérifie la transition vers Completed après succès |
| `QuestProgressFailedEvent_ShouldTransitionToFailed` | Vérifie la transition vers Failed après échec |
| `MultipleSagas_ShouldBeIndependent` | Vérifie que plusieurs sagas fonctionnent indépendamment |

### Tests du Consumer (`GameCompletedConsumerTests.cs`)

| Test | Description |
|------|-------------|
| `Consume_ShouldUpdateQuestProgress_WhenMatchingQuestExists` | Vérifie la mise à jour de la progression |
| `Consume_ShouldCompleteQuest_WhenTargetReached` | Vérifie la complétion de quête |
| `Consume_ShouldBeIdempotent_WhenSameEventProcessedTwice` | **Test d'idempotence** - même événement = 1 seul traitement |
| `Consume_ShouldPublishSuccessEvent_WhenNoMatchingQuest` | Vérifie la publication même sans quête |
| `Consume_ShouldRecordInInboxState_ForIdempotence` | Vérifie l'enregistrement dans l'Inbox |

