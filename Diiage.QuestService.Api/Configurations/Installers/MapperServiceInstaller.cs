using System.Reflection;
using Diiage.QuestService.Application;
using Mapster;
using MapsterMapper;
using PierreProject.Core.Api.Configurations;
using ILogger = Serilog.ILogger;

namespace Diiage.QuestService.Api.Configurations.Installers;

public class MapperServiceInstaller : IServiceInstaller
{
    /// <inheritdoc />
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        var config = new TypeAdapterConfig();

        config.Default.AddDestinationTransform(DestinationTransform.EmptyCollectionIfNull);
        services.AddApplicationMapping(config);

        config.Scan(Assembly.GetExecutingAssembly());

        config.Compile();
        services.AddSingleton(config);
        services.AddScoped<IMapper, Mapper>();
    }
}