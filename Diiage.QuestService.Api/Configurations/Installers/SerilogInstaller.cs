using PierreProject.Core.Api.Configurations;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Diiage.QuestService.Api.Configurations.Installers;

public class SerilogInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        Log.Logger = loggerConfig.CreateLogger();
    }
}