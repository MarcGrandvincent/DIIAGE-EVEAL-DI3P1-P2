using Microsoft.AspNetCore.Mvc.Razor;
using PierreProject.Common.Api.Filters;
using PierreProject.Core.Api.Configurations;
using ILogger = Serilog.ILogger;

namespace Diiage.QuestService.Api.Configurations.Installers;

public class ControllerServiceInstaller : IServiceInstaller
{
    /// <inheritdoc />
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddControllers(options =>
            {
                options.Filters.Add<RequestBodyActionFilter>();
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; })
            .AddApplicationPart(typeof(Program).Assembly)
            .AddMvcLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddNewtonsoftJson();
    }
}