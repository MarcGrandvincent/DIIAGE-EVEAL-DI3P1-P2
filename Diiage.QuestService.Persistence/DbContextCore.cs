using Diiage.QuestService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Diiage.QuestService.Persistence;

public class DbContextCore : DbContext
{
    public DbContextCore()
    {
    }

    public DbContextCore(DbContextOptions<DbContextCore> dbContextOptions)
        : base(dbContextOptions)
    {
    }
    
    /// <summary>
    /// Table Inbox pour l'idempotence des messages.
    /// </summary>
    public DbSet<InboxState> InboxStates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("diiage-questservice-db");

        builder.ApplyConfigurationsFromAssembly(typeof(DbContextCore).Assembly);
    }
}