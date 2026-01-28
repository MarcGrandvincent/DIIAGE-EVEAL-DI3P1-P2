using Diiage.QuestService.Repositories;
using Diiage.QuestService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thinktecture;

namespace Diiage.QuestService.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration appSettings)
    {
        services.ConfigureDbContext(appSettings["Database:ConnectionString"] ??
                                    throw new ArgumentNullException("Database:ConnectionString"));
        RegisterRepositories(services);

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork<DbContextCore>>();
    }

    private static void ConfigureDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DbContextCore>(optionsBuilder =>
        {
            optionsBuilder.UseSqlServer(connectionString, sqlBuilder =>
            {
                sqlBuilder.MigrationsAssembly("Diiage.QuestService.Persistence.Migrations");
                sqlBuilder.MigrationsHistoryTable("__EFMigrationsHistory");
                sqlBuilder.AddTableHintSupport();
            });
        });
    }

    /// <summary>
    ///     Applique les migrations EF Core au démarrage de l'application.
    /// </summary>
    /// <param name="serviceProvider">Le conteneur de services de l'application.</param>
    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetService<ILogger<DbContextCore>>();

        try
        {
            logger?.LogInformation("Applying database migrations on startup.");

            var dbContext = services.GetRequiredService<DbContextCore>();
            dbContext.Database.Migrate();

            logger?.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while applying database migrations on startup.");
            throw;
        }
    }
}