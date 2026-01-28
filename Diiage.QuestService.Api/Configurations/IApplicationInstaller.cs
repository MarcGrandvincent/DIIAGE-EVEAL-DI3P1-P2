namespace PierreProject.Core.Api.Configurations;

public interface IApplicationInstaller
{
    void Setup(WebApplication application, IConfiguration configuration);
}