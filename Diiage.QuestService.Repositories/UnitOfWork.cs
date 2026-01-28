using Diiage.QuestService.Repositories.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Diiage.QuestService.Repositories;

public class UnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
{
    private readonly DbContext _context;
    private bool _disposed;
    private readonly Dictionary<Type, object> _repositories;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork{TDBContext}"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="mapper">The mapper.</param>
    public UnitOfWork(TDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _repositories = new();
    }

    public DbContext DbContext => _context;

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<TEntity>(_context, _mapper);
        }

        return (IGenericRepository<TEntity>)_repositories[type];
    }

    public int ExecuteSqlCommand(
        string sql,
        params object[] parameters
    ) => _context.Database.ExecuteSqlRaw(sql, parameters);

    public IQueryable<TEntity> FromSql<TEntity>(
        string sql,
        params object[] parameters
    ) where TEntity : class => _context.Set<TEntity>().FromSqlRaw(sql, parameters);

    public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _repositories.Clear();
                _context.Dispose();
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}