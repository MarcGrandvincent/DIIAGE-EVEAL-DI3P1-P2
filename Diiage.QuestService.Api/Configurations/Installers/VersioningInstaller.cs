using System.Reflection;
using Asp.Versioning;
using PierreProject.Core.Api.Configurations;
using ILogger = Serilog.ILogger;

namespace Diiage.QuestService.Api.Configurations.Installers;

public class VersioningInstaller : IServiceInstaller
{
    public static (string informational, string? commit, string shortCommit, string fileVersion, string asmVersion,
        DateTime? buildTime) ReadAssemblyVersion()
    {
        // Avec Minimal API, GetExecutingAssembly() est fiable
        var asm = Assembly.GetExecutingAssembly();

        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
        var file = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "";
        var ver = asm.GetName().Version?.ToString() ?? "";

        string? commit = null;
        if (!string.IsNullOrEmpty(info))
        {
            var idx = info.IndexOf('+');
            if (idx >= 0 && idx < info.Length - 1)
                commit = info[(idx + 1)..]; // suffixe après '+'
        }

        var shortCommit = string.IsNullOrEmpty(commit) ? "" : commit[..Math.Min(7, commit.Length)];

        DateTime? buildTime = null;
        try
        {
            var loc = asm.Location;
            if (!string.IsNullOrEmpty(loc))
                buildTime = File.GetLastWriteTimeUtc(loc);
        }
        catch
        {
            /* sandbox scenarios */
        }

        return (info, commit, shortCommit, file, ver, buildTime);
    }

    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}