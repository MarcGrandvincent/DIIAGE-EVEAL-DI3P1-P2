using Diiage.QuestService.Application.Consumers;
using Diiage.QuestService.Application.Sagas;
using MassTransit;

namespace Diiage.QuestService.Api.Configurations.Installers;

public static class RabbitMqInstaller
{
    public static void SetupRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
// Configuration MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            // Enregistrer le consumer
            x.AddConsumer<GameCompletedConsumer>();

            // Enregistrer la Saga State Machine
            x.AddSagaStateMachine<QuestProgressSaga, QuestProgressSagaState>()
                .InMemoryRepository(); 

            // Configure RabbitMQ comme transport
            x.UsingRabbitMq((context, cfg) =>
            {
                // URL du RabbitMQ (dev: localhost:5672, prod: service name)
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitMqPort = ushort.Parse(configuration["RabbitMQ:Port"] ?? "5672");

                cfg.Host(rabbitMqHost, rabbitMqPort, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // Configure les endpoints pour l'auto-wiring des consumers
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}