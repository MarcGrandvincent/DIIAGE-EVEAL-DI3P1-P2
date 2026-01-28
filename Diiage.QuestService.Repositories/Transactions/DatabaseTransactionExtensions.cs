using Diiage.QuestService.Repositories.Interfaces;

namespace Diiage.QuestService.Repositories.Transactions;

/// <summary>
///     Contains all extensions methods for managing database transactions.
/// </summary>
public static class DatabaseTransactionExtensions
{
    /// <summary>
    ///     Starts a new transaction with the willing to insert new data.
    ///     If a transaction already exists on the database, it will be used.
    /// </summary>
    /// <remarks>
    ///     This will lock the entire table, keep this transaction short!
    /// </remarks>
    /// <param name="repository">Repository used as a reference to the DbContext in-use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <returns>Transaction used.</returns>
    public static async Task<DatabaseTransaction> BeginTransactionForInsert<T>(this IGenericRepository<T> repository,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var tx = new DatabaseTransaction(repository.DbContext);
        if (tx.IsNewTransaction)
            await repository.LockEntityTableAsync(cancellationToken);

        return tx;
    }

    /// <summary>
    ///     Starts a new transaction with the willing to insert new data.
    ///     If a transaction already exists on the database, it will be used.
    /// </summary>
    /// <remarks>
    ///     This will lock the entire table, keep this transaction short!
    /// </remarks>
    /// <param name="repository">Repository used as a reference to the DbContext in-use.</param>
    /// <param name="primaryKey">Primary key column name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <returns>Transaction used.</returns>
    public static async Task<DatabaseTransaction> BeginTransactionForInsert<T>(this IGenericRepository<T> repository,
        string primaryKey, CancellationToken cancellationToken = default)
        where T : class
    {
        var tx = new DatabaseTransaction(repository.DbContext);
        if (tx.IsNewTransaction)
            await repository.LockEntityTableAsync(primaryKey, cancellationToken);

        return tx;
    }

    /// <summary>
    ///     Starts a new transaction with the willing to update data on TPT hierarchy.
    ///     If a transaction already exists on the database, it will be used.
    /// </summary>
    /// <remarks>
    ///     This will lock the entire table, keep this transaction short!
    /// </remarks>
    /// <param name="repository">Repository used as a reference to the DbContext in-use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <typeparam name="K">Entity base type which will be locked.</typeparam>
    /// <returns>Transaction used.</returns>
    public static async Task<DatabaseTransaction> BeginTransactionForUpdateOnTpt<T, K>(
        this IGenericRepository<T> repository, CancellationToken cancellationToken = default)
        where T : class, K
        where K : class
    {
        var tx = new DatabaseTransaction(repository.DbContext);
        if (tx.IsNewTransaction)
            await repository.LockEntityTableAsync<K>(cancellationToken);

        return tx;
    }

    /// <summary>
    ///     Starts a new transaction with the willing to update data on TPT hierarchy.
    ///     If a transaction already exists on the database, it will be used.
    /// </summary>
    /// <remarks>
    ///     This will lock the entire table, keep this transaction short!
    /// </remarks>
    /// <param name="repository">Repository used as a reference to the DbContext in-use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">Entity type which will be locked.</typeparam>
    /// <returns>Transaction used.</returns>
    public static async Task<DatabaseTransaction> BeginTransactionForUpdateOnTpt<T>(
        this IGenericRepository<T> repository, CancellationToken cancellationToken = default)
        where T : class
    {
        var tx = new DatabaseTransaction(repository.DbContext);
        if (tx.IsNewTransaction)
            await repository.LockEntityTableAsync(cancellationToken);

        return tx;
    }

    /// <summary>
    ///     Starts a new transaction.
    ///     If a transaction already exists on the database, it will be used.
    /// </summary>
    /// <param name="repository">Repository used as a reference to the DbContext in-use.</param>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <returns>Transaction used.</returns>
    public static DatabaseTransaction BeginTransaction<T>(this IGenericRepository<T> repository)
        where T : class
    {
        return new DatabaseTransaction(repository.DbContext);
    }
}