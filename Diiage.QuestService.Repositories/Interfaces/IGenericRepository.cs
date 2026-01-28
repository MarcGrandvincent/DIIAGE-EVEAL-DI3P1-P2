using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Thinktecture.EntityFrameworkCore;

namespace Diiage.QuestService.Repositories.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    /// <summary>
    ///     DbContext used to get datasets and query against the database.
    /// </summary>
    DbContext DbContext { get; }

    #region CREATE

    /// <summary>
    ///     Inserts a new entity.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    TEntity Add(TEntity entity);

    /// <summary>
    ///     Inserts a new entity.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <returns>An instance of TEntity.</returns>
    TEntity Add(TEntity entity, SqlServerTableHint[] tableHints);

    /// <summary>
    ///     Inserts a range of entities.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    void Add(IEnumerable<TEntity> entities);

    /// <summary>
    ///     Inserts new entities.
    /// </summary>
    /// <param name="entities">Entities to insert.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <returns>An instance of TEntity.</returns>
    void Add(IEnumerable<TEntity> entities, SqlServerTableHint[] tableHints);

    #endregion

    #region READ

    /// <summary>
    ///     Finds an entity with the given primary key values.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="keyValues">The values of the primary key.</param>
    /// <returns>The found entity or null.</returns>
    Task<TEntity?> GetByIdAsync(CancellationToken cancellationToken = default, params object?[]? keyValues);

    /// <summary>
    ///     Gets the first or default entity based on a predicate, orderby and children inclusions.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="include">Navigation properties separated by a comma.</param>
    /// <param name="disableTracking">A boolean to disable entities changing tracking.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first element satisfying the condition.</returns>
    /// <remarks>This method default no-tracking query.</remarks>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Gets the first or default entity based on a predicate, orderby and children inclusions.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="projection">A function to declare a projection.</param>
    /// <param name="disableTracking">A boolean to disable entities changing tracking.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first projected element satisfying the condition.</returns>
    /// <remarks>This method default no-tracking query.</remarks>
    Task<T?> GetFirstOrDefaultAsync<T>(
        Expression<Func<TEntity, T>> projection,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Gets the first or default entity based on a predicate, orderby and children inclusions.
    ///     Map result to class given by generic
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="include">Navigation properties separated by a comma.</param>
    /// <param name="disableTracking">A boolean to disable entities changing tracking.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first element satisfying the condition, mapped to the generic type given.</returns>
    /// <remarks>This method default no-tracking query.</remarks>
    Task<TProjection?> GetFirstOrDefaultAsync<TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default
    );
    
    /// <summary>
    ///     Gets the first or default entity based on a queryable.
    /// </summary>
    /// <param name="queryable">The queryable to query against.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first element satisfying the condition, mapped to the generic type given.</returns>
    Task<TEntity?> GetFirstOrDefaultAsync(IQueryable<TEntity> queryable,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the <see cref="IQueryable{TEntity}"/> of the <see cref="DbSet{TEntity}"/>.
    /// </summary>
    /// <returns>The queryable.</returns>
    IQueryable<TEntity> Queryable { get; }

    /// <summary>
    ///     Gets the entities based on a predicate, orderby and children inclusions.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="include">A function to include navigation properties</param>
    /// <param name="disableTracking">A boolean to disable entities changing tracking.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of elements satisfying the condition.</returns>
    /// <remarks>This method defaults to a tracking query.</remarks>
    Task<List<TEntity>> GetMultipleAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the entities based on a queryable.
    /// </summary>
    /// <param name="queryable">The queryable to query against.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of elements satisfying the queryable.</returns>
    Task<List<TEntity>> GetMultipleAsync(IQueryable<TEntity> queryable,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the entities based on a queryable.
    /// </summary>
    /// <param name="queryable">The queryable to query against.</param>
    /// <param name="projection">A function to declare a projection.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of elements satisfying the queryable.</returns>
    Task<List<T>> GetMultipleAsync<T>(IQueryable<TEntity> queryable,
        Expression<Func<TEntity, T>> projection,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the entities based on a queryable.
    /// </summary>
    /// <param name="projection">A function to declare a projection.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="disableTracking">A boolean to disable entities changing tracking.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="tableHints">SQL Server table hints (if supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of elements satisfying the queryable.</returns>
    Task<List<T>> GetMultipleAsync<T>(Expression<Func<TEntity, T>> projection,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the entities based on a predicate, orderby and children inclusions.
    ///     Map result to class given by generic.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="include">A function to include navigation properties</param>
    /// <param name="disableTracking">A boolean to disable entities changing tracking.</param>
    /// <param name="tableHints"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of elements satisfying the condition, mapped to the generic type given.</returns>
    /// <remarks>This method default no-tracking query.</remarks>
    Task<List<TProjection>> GetMultipleAsync<TProjection>(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uses raw SQL queries to fetch the specified entity data.
    /// </summary>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>A list of elements satisfying the condition specified by raw SQL.</returns>
    IQueryable<TEntity> FromSqlRaw(string sql, params object[] parameters);

    #endregion

    #region UPDATE

    /// <summary>
    ///     Updates the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    TEntity Update(TEntity entity);

    /// <summary>
    ///     Updates the specified entities.
    /// </summary>
    /// <param name="entities">The entities.</param>
    void Update(IEnumerable<TEntity> entities);

    #endregion

    #region DELETE

    /// <summary>
    ///     Deletes the entity by the specified primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the specified entity.
    /// </summary>
    /// <param name="entityToDelete">The entity to delete.</param>
    void Delete(TEntity entityToDelete);

    /// <summary>
    ///     Delete the specified entities.
    /// </summary>
    /// <param name="entities">Entities to delete.</param>
    void Delete(IEnumerable<TEntity> entities);

    #endregion

    #region OTHER

    /// <summary>
    ///     Locks entity table.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task LockEntityTableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Locks specified entity table higher in the inheritance hierarchy.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">The type of the entity to lock.</typeparam>
    /// <returns>Task.</returns>
    Task LockEntityTableAsync<T>(CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    ///     Locks entity table.
    /// </summary>
    /// <param name="primaryKey">Primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task LockEntityTableAsync(string primaryKey = "Id", CancellationToken cancellationToken = default);

    /// <summary>
    ///     Locks specified entity table higher in the inheritance hierarchy.
    /// </summary>
    /// <param name="primaryKey">Primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">The type of the entity to lock.</typeparam>
    /// <returns>Task.</returns>
    Task LockEntityTableAsync<T>(string primaryKey = "Id", CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    ///     Gets the count based on a predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of rows.</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Check if an element exists following a condition.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>If it exists.</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    #endregion
}