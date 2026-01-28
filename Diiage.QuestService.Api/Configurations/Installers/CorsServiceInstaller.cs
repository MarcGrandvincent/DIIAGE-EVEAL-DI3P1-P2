using PierreProject.Core.Api.Configurations;
using ILogger = Serilog.ILogger;

namespace Diiage.QuestService.Api.Configurations.Installers;

public class CorsServiceInstaller : IServiceInstaller, IApplicationInstaller
{
    private const string CorsPolicyName = "AllowConfiguredOrigins";

    public void Setup(WebApplication app, IConfiguration configuration)
    {
        app.UseCors(CorsPolicyName);
    }

    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName,
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
    }
}