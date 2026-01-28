using System.Reflection;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PierreProject.Common.Mediator.Behaviors;

namespace Diiage.QuestService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register all FluentValidation validators in this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services.AddMediatR(cf =>
        {
            cf.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            cf.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cf.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
    }

    public static void AddApplicationMapping(this IServiceCollection services, TypeAdapterConfig config)
    {
        config.Scan(Assembly.GetExecutingAssembly());
        /**config.Scan(Assembly.GetAssembly(typeof(Item)) ??
                    throw new ApplicationException("Could not get Domain assembly to register mapping"));**/
    }
}