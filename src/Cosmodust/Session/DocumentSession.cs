using Cosmodust.Linq;
using Cosmodust.Query;
using Cosmodust.Serialization;
using Cosmodust.Store;
using Cosmodust.Tracking;

namespace Cosmodust.Session;

/// <summary>
/// Represents a session for interacting with the Cosmos DB database.
/// </summary>
public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(
        ChangeTracker changeTracker,
        IDatabase database,
        EntityConfigurationHolder entityConfiguration,
        SqlParameterCache sqlParameterCache)
    {
        Database = database;
        ChangeTracker = changeTracker;
        EntityConfiguration = entityConfiguration;
        SqlParameterCache = sqlParameterCache;
    }

    public ChangeTracker ChangeTracker { get; }
    public IDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }
    public SqlParameterCache SqlParameterCache { get; }

    /// <summary>
    /// Finds an entity of type <typeparamref name="TEntity"/> by its ID and partition key.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to find.</typeparam>
    /// <param name="id">The ID of the entity to find.</param>
    /// <param name="partitionKey">The partition key of the entity to find.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask{TEntity}"/> representing the asynchronous operation, containing the found entity, or <c>null</c> if the entity was not found.</returns>
    public async ValueTask<TEntity?> FindAsync<TEntity>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        var configuration = GetConfiguration<TEntity>();
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        if (ChangeTracker.TryGet(id, out TEntity? entity))
            return entity;

        entity = await Database.FindAsync<TEntity>(
            configuration.ContainerName,
            id,
            partitionKey,
            cancellationToken);

        if (entity is null)
            return default;

        ChangeTracker.RegisterUnchanged(entity);

        return entity;
    }

    /// <summary>
    /// Returns a queryable object for the specified entity type and partition key.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query.</typeparam>
    /// <param name="partitionKey">The partition key to use for the query.</param>
    /// <returns>A queryable object for the specified entity type and partition key.</returns>
    public IQueryable<TEntity> Query<TEntity>(string partitionKey)
    {
        var entityConfiguration = GetConfiguration<TEntity>();
        var queryable = Database.CreateLinqQuery<TEntity>(entityConfiguration.ContainerName);

        return new CosmodustLinqQuery<TEntity>(
            Database,
            ChangeTracker,
            entityConfiguration,
            partitionKey,
            queryable);
    }

    /// <summary>
    /// Executes a SQL query against the Cosmos DB database and returns the results as a <see cref="SqlQuery{TEntity}"/> object.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query.</typeparam>
    /// <param name="partitionKey">The partition key value for the query.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">An optional object containing parameters to be used in the query.</param>
    /// <returns>A <see cref="SqlQuery{TEntity}"/> object representing the results of the query.</returns>
    public SqlQuery<TEntity> Query<TEntity>(
        string partitionKey,
        string sql,
        object? parameters = null)
    {
        var config = GetConfiguration<TEntity>();

        return new SqlQuery<TEntity>(
            database: Database,
            changeTracker: ChangeTracker,
            entityConfiguration: config,
            sql: sql,
            partitionKey: partitionKey,
            parameters: SqlParameterCache.ExtractParametersFromObject(parameters));
    }

    /// <summary>
    /// Stores the specified entity in the session.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to store.</typeparam>
    /// <param name="entity">The entity to store.</param>
    public void Store<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterAdded(entity);
    }

    /// <summary>
    /// Updates the specified entity in the change tracker.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to update.</param>
    public void Update<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterModified(entity);
    }

    /// <summary>
    /// Removes the specified entity from the session.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to remove.</typeparam>
    /// <param name="entity">The entity to remove.</param>
    public void Remove<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterRemoved(entity);
    }

    /// <summary>
    /// Commits all pending changes made in the session to the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);
        ChangeTracker.Commit();
    }

    /// <summary>
    /// Commits the pending changes in the current transaction to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);
        ChangeTracker.Commit();
    }

    private EntityConfiguration GetConfiguration<TEntity>() =>
        EntityConfiguration.Get(typeof(TEntity)) ??
        throw new InvalidOperationException($"No configuration has been registered for type {typeof(TEntity).FullName}.");
}
