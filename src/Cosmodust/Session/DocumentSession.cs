using System.Diagnostics;
using Cosmodust.Linq;
using Cosmodust.Query;
using Cosmodust.Serialization;
using Cosmodust.Shared;
using Cosmodust.Store;
using Cosmodust.Tracking;

namespace Cosmodust.Session;

public sealed class DocumentSession : IDocumentSession, IDisposable
{
    internal DocumentSession(
        IDatabase database,
        EntityConfigurationProvider entityConfiguration,
        SqlParameterObjectTypeCache sqlParameterObjectTypeCache,
        ShadowPropertyStore shadowPropertyStore)
    {
        Database = database;
        EntityConfiguration = entityConfiguration;
        SqlParameterObjectTypeCache = sqlParameterObjectTypeCache;
        ChangeTracker = new ChangeTracker(
            entityConfiguration,
            shadowPropertyStore);
    }

    public ChangeTracker ChangeTracker { get; }
    public IDatabase Database { get; }
    public EntityConfigurationProvider EntityConfiguration { get; }
    public SqlParameterObjectTypeCache SqlParameterObjectTypeCache { get; }

    /// <inheritdoc />
    public async ValueTask<TEntity?> FindAsync<TEntity>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrWhiteSpace(id);
        Ensure.NotNullOrWhiteSpace(partitionKey);

        var configuration = GetConfiguration<TEntity>();

        if (ChangeTracker.TryGet(id, out TEntity? entity))
            return entity;

        var readOperationResult = await Database.FindAsync<TEntity>(
            configuration.ContainerName,
            id,
            partitionKey,
            cancellationToken);

        entity = (TEntity?) readOperationResult.Entity;

        if (entity is null)
            return default;

        ChangeTracker.RegisterUnchanged(entity, readOperationResult.ETag);

        return entity;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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
            parameters: SqlParameterObjectTypeCache.ExtractParametersFromObject(parameters));
    }

    /// <inheritdoc />
    public void Store<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterAdded(entity);
    }

    /// <inheritdoc />
    public void Update<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterModified(entity);
    }

    /// <inheritdoc />
    public void Remove<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterRemoved(entity);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);
        ChangeTracker.Commit();
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);
        ChangeTracker.Commit();
    }

    public EntityEntry Entity(object entity) =>
        ChangeTracker.Entry(entity);

    public void Dispose()
    {
        ChangeTracker.Dispose();
    }

    private EntityConfiguration GetConfiguration<TEntity>() =>
        EntityConfiguration.GetEntityConfiguration(typeof(TEntity)) ??
        throw new InvalidOperationException($"No configuration has been registered for type {typeof(TEntity).FullName}.");
}
