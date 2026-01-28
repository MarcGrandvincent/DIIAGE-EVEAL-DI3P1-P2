using Diiage.QuestService.Api.Configurations.Installers;
using Diiage.QuestService.Api.Configurations.Installers.ProblemsDetails;
using Diiage.QuestService.Application;
using Diiage.QuestService.Persistence;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PierreProject.Core.Api.Configurations;
using PierreProject.Core.Api.Configurations.Middleware;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.SetupProblemDetails(builder.Environment.IsDevelopment());
builder.Services.SetupRabbitMq(builder.Configuration);

// OpenAPI + Scalar
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication(builder.Configuration)
    .AddMemoryCache()
    .InstallServices(
        builder.Configuration,
        Log.Logger.ForContext<IServiceInstaller>(),
        typeof(IServiceInstaller).Assembly);

builder.Host.UseSerilog();


builder.Services.AddAuthorization();

var app = builder.Build();

// Appliquer les migrations EF Core au démarrage
app.Services.ApplyMigrations();

app.InstallApps(builder.Configuration);

app.MapGet("/version", () =>
{
    var v = VersioningInstaller.ReadAssemblyVersion();
    return Results.Ok(new
    {
        informationalVersion = v.informational,
        gitCommit = v.commit ?? "",
        v.shortCommit,
        v.fileVersion,
        assemblyVersion = v.asmVersion,
        buildTimeUtc = v.buildTime?.ToString("O")
    });
});

app.UseMiddleware<InvalidRequestBodyMiddleware>();

app.UseProblemDetails();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var isHealthy = report.Status == HealthStatus.Healthy;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(isHealthy ? "1" : "0");
    }
});

app.MapControllers();

app.UseCors("AllowConfiguredOrigins");

app.Run();