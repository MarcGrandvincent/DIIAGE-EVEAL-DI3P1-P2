using Hellang.Middleware.ProblemDetails;

namespace Diiage.QuestService.Api.Configurations.Installers.ProblemsDetails;

public static class ProblemsDetailsInstaller
{
    public static IServiceCollection SetupProblemDetails(this IServiceCollection services, bool isDevEnv)
    {
        var problemsConfiguration = new ProblemsConfiguration(isDevEnv);

        services.AddProblemDetails(o => { problemsConfiguration.ConfigureProblemDetails(o); });

        return services;
    }
}