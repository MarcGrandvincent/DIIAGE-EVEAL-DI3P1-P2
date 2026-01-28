using ILogger = Serilog.ILogger;

namespace PierreProject.Core.Api.Configurations;

public interface IServiceInstaller
{
    void Install(IServiceCollection services, IConfiguration configuration, ILogger logger);
}