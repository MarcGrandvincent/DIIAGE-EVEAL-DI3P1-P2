using System.Reflection;
using ILogger = Serilog.ILogger;

namespace PierreProject.Core.Api.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection InstallServices(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        params Assembly[] assemblies)
    {
        var serviceInstallers = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(IsAssignableToType<IServiceInstaller>)
            .Select(Activator.CreateInstance)
            .Cast<IServiceInstaller>();

        foreach (var serviceInstaller in serviceInstallers)
            serviceInstaller.Install(services, configuration, logger);

        return services;
    }

    public static WebApplication InstallApps(
        this WebApplication app,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var serviceInstallers = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(IsAssignableToType<IApplicationInstaller>)
            .Select(Activator.CreateInstance)
            .Cast<IApplicationInstaller>();

        foreach (var serviceInstaller in serviceInstallers)
            serviceInstaller.Setup(app, configuration);

        return app;
    }

    private static bool IsAssignableToType<T>(TypeInfo typeInfo)
    {
        return typeof(T).IsAssignableFrom(typeInfo) &&
               typeInfo is { IsInterface: false, IsAbstract: false };
    }
}