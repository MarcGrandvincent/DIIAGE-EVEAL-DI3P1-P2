using System.Linq.Expressions;
using Diiage.QuestService.Repositories.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Thinktecture;
using Thinktecture.EntityFrameworkCore;

namespace Diiage.QuestService.Repositories;

/// <summary>
///     Dépôt pour <typeparamref name="TEntity" />.
/// </summary>
/// <typeparam name="TEntity">L'entité que le dépôt gère.</typeparam>
public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly IMapper _mapper;

    public DbContext DbContext => _dbContext;

    /// <summary>
    ///     Initializes a new instance of the GenericRepository.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="mapper">Mapper.</param>
    public GenericRepository(DbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<TEntity>();
        _mapper = mapper;
    }

    #region CREATE

    public virtual TEntity Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Add(entity);

        return entity;
    }

    public virtual TEntity Add(TEntity entity, SqlServerTableHint[] tableHints)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.WithTableHints(tableHints);
        Add(entity);

        return entity;
    }

    public virtual void Add(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        _dbSet.AddRange(entities);
    }

    public virtual void Add(IEnumerable<TEntity> entities, SqlServerTableHint[] tableHints)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(tableHints);

        _dbSet.WithTableHints(tableHints);
        Add(entities);
    }

    #endregion

    #region READ

    public virtual async Task<TEntity?> GetByIdAsync(CancellationToken cancellationToken = default,
        params object?[]? keyValues)
    {
        return await _dbSet.FindAsync(keyValues, cancellationToken);
    }

    public virtual Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = tableHints is not null ? _dbSet.WithTableHints(tableHints) : _dbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        if (include != null)
            query = include(query);

        if (orderBy != null)
            return orderBy(query).FirstOrDefaultAsync(cancellationToken);

        return predicate != null
            ? query.FirstOrDefaultAsync(predicate, cancellationToken)
            : query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<T?> GetFirstOrDefaultAsync<T>(Expression<Func<TEntity, T>> projection,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool disableTracking = false, SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default)
    {
        var query = tableHints is not null ? _dbSet.WithTableHints(tableHints) : _dbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            query = orderBy(query);

        return query.Select(projection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TProjection?> GetFirstOrDefaultAsync<TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default
    )
    {
        TProjection? projectTo = default;
        var entity =
            await GetFirstOrDefaultAsync(predicate, orderBy, include, disableTracking, tableHints,
                cancellationToken);

        if (entity != null)
            projectTo = _mapper.Map<TProjection?>(entity);

        return projectTo;
    }

    public IQueryable<TEntity> Queryable
    {
        get => _dbSet;
    }

    public virtual async Task<List<TEntity>> GetMultipleAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default)
    {
        var query = tableHints is not null ? _dbSet.WithTableHints(tableHints) : _dbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        if (include != null)
            query = include(query);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            return await orderBy(query).ToListAsync(cancellationToken);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetMultipleAsync(IQueryable<TEntity> queryable,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default)
    {
        queryable = tableHints is not null ? queryable.WithTableHints(tableHints) : queryable;

        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync(IQueryable<TEntity> queryable,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default)
    {
        queryable = tableHints is not null ? queryable.WithTableHints(tableHints) : queryable;

        return await queryable.FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<T>> GetMultipleAsync<T>(IQueryable<TEntity> queryable,
        Expression<Func<TEntity, T>> projection,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default)
    {
        queryable = tableHints is not null ? queryable.WithTableHints(tableHints) : queryable;

        if (orderBy != null)
            queryable = orderBy(queryable);

        return await queryable.Select(projection).ToListAsync(cancellationToken);
    }

    public async Task<List<T>> GetMultipleAsync<T>(Expression<Func<TEntity, T>> projection,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = tableHints is not null ? _dbSet.WithTableHints(tableHints) : _dbSet;
        
        if (disableTracking)
            queryable = queryable.AsNoTracking();

        if (predicate != null)
            queryable = queryable.Where(predicate);

        if (orderBy != null)
            queryable = orderBy(queryable);

        return await queryable.Select(projection).ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TProjection>> GetMultipleAsync<TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = false,
        SqlServerTableHint[]? tableHints = null,
        CancellationToken cancellationToken = default)
    {
        var entities =
            await GetMultipleAsync(predicate, orderBy, include, disableTracking, tableHints, cancellationToken);

        return _mapper.Map<List<TProjection>>(entities);
    }

    public virtual IQueryable<TEntity> FromSqlRaw(string sql, params object[] parameters)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        return _dbSet.FromSqlRaw(sql, parameters);
    }

    #endregion

    #region UPDATE

    public virtual TEntity Update(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _dbSet.Update(entity);

        return entity;
    }

    public virtual void Update(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    #endregion

    #region DELETE

    public virtual async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        var entityToDelete = await _dbSet.FindAsync(new[] { id }, cancellationToken);
        if (entityToDelete != null)
            _dbSet.Remove(entityToDelete);
    }

    public virtual void Delete(TEntity entityToDelete)
    {
        if (_dbContext.Entry(entityToDelete).State == EntityState.Detached) _dbSet.Attach(entityToDelete);

        _dbSet.Remove(entityToDelete);
    }

    public virtual void Delete(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    #endregion

    #region OTHER

    public Task LockEntityTableAsync(CancellationToken cancellationToken = default)
        => LockEntityTableAsync("Id", cancellationToken);

    public Task LockEntityTableAsync<T>(CancellationToken cancellationToken = default) where T : class
        => LockEntityTableAsync<T>("Id", cancellationToken);

    public virtual Task LockEntityTableAsync(string primaryKey = "Id",
        CancellationToken cancellationToken = default)
    {
        var mapping = _dbContext.Model.FindEntityType(typeof(TEntity));
        if (mapping is null)
            throw new ApplicationException("Could not find entity of type T");

        return ExecuteSqlLockCommandForEntityAsync(mapping, primaryKey, cancellationToken);
    }

    public virtual Task LockEntityTableAsync<T>(string primaryKey = "Id",
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (!typeof(T).IsAssignableFrom(typeof(TEntity)))
            throw new Exception("Type is not assignable to inherited entity");

        var mapping = _dbContext.Model.FindEntityType(typeof(T));
        if (mapping is null)
            throw new ApplicationException("Could not find entity of type T");

        return ExecuteSqlLockCommandForEntityAsync(mapping, primaryKey, cancellationToken);
    }

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? _dbSet.CountAsync(cancellationToken)
            : _dbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return _dbSet.AnyAsync(predicate, cancellationToken);
    }

    #endregion

    private async Task ExecuteSqlLockCommandForEntityAsync(IReadOnlyEntityType entityType, string primaryKey,
        CancellationToken cancellationToken = default)
    {
        var schema = entityType.GetSchema();
        var tableName = entityType.GetTableName();
        if (schema is null || tableName is null)
            throw new ApplicationException("Could not find table name or schema");

        var tableNameQuery = $"[{schema}].[{tableName}]";
        var query = $"SELECT TOP 1 [{primaryKey}] FROM {tableNameQuery} WITH (TABLOCKX, HOLDLOCK)";

        await _dbContext.Database.ExecuteSqlRawAsync(query, cancellationToken: cancellationToken);
    }
}