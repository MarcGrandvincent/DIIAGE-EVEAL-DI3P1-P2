using Diiage.QuestService.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PierreProject.Core.Api.Configurations;
using ILogger = Serilog.ILogger;

namespace Diiage.QuestService.Api.Configurations.Installers;

public class HealthCheckInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<DbContextCore>(
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "efcore" });
    }
}