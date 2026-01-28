using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Thinktecture.EntityFrameworkCore;
using IsolationLevel = System.Data.IsolationLevel;

namespace Diiage.QuestService.Repositories.Transactions;

/// <summary>
///     Creates a new transaction with the database for the current scope.
/// </summary>
public class DatabaseTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///     SQL table hints for update when search is not made on an index.
    /// </summary>
    public static readonly SqlServerTableHint[] ForUpdateHints =
        { SqlServerTableHint.TabLockx, SqlServerTableHint.UpdLock };

    /// <summary>
    ///     SQL table hints for update when discriminant is on an index.
    /// </summary>
    public static readonly SqlServerTableHint[] ForUpdateHintsForIndex =
        { SqlServerTableHint.RowLock, SqlServerTableHint.UpdLock, SqlServerTableHint.HoldLock };

    public IDbContextTransaction ContextTransaction { get; }

    /// <summary>
    ///     Initialize a new transaction.
    /// </summary>
    /// <param name="dbContext">DbContext to initialize a new transaction on.</param>
    public DatabaseTransaction(DbContext dbContext) : this(dbContext, IsolationLevel.ReadCommitted)
    {
    }

    /// <summary>
    ///     Initialize a new transaction with an isolation level.
    /// </summary>
    /// <param name="dbContext">DbContext to initialize a new transaction on.</param>
    /// <param name="isolationLevel">Isolation level.</param>
    protected DatabaseTransaction(DbContext dbContext, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (dbContext.Database.GetEnlistedTransaction() != null)
            return;

        if (Transaction.Current is not null)
            return;

        if (dbContext.Database.CurrentTransaction is { } currTransac)
        {
            ContextTransaction = currTransac;
        }
        else
        {
            ContextTransaction = dbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            IsNewTransaction = true;
        }
    }

    /// <summary>
    ///     Is the current transaction created for the current instance of this <see cref="DatabaseTransaction"/> ?
    /// </summary>
    public bool IsNewTransaction { get; }

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        ContextTransaction.CommitAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (IsNewTransaction)
            await ContextTransaction.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (IsNewTransaction)
            ContextTransaction.Dispose();

        GC.SuppressFinalize(this);
    }
}