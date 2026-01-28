using Microsoft.EntityFrameworkCore;

namespace Diiage.QuestService.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    ///     Used DB Context.
    /// </summary>
    DbContext DbContext { get; }

    /// <summary>
    ///     Gets the specified repository for the TEntity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>An instance of type inherited from GenericRepository interface.</returns>
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    ///     Executes the specified raw SQL command.
    /// </summary>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The number rows affected.</returns>
    int ExecuteSqlCommand(string sql, params object[] parameters);
    
    /// <summary>
    ///     Commit all changes made in this context to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}