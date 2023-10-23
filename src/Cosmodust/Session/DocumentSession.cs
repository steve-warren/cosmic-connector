using Cosmodust.Linq;
using Cosmodust.Query;
using Cosmodust.Store;
using Cosmodust.Tracking;

namespace Cosmodust.Session;

public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(DocumentStore documentStore, ChangeTracker changeTracker, IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(documentStore);

        DocumentStore = documentStore;
        Database = database;
        ChangeTracker = changeTracker;
    }

    public ChangeTracker ChangeTracker { get; }
    public DocumentStore DocumentStore { get; }
    public IDatabase Database { get; }

    public async ValueTask<TEntity?> FindAsync<TEntity>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        var configuration = DocumentStore.GetConfiguration<TEntity>();
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        if (ChangeTracker.TryGet(id, out TEntity? entity))
            return entity;

        entity = await Database.FindAsync<TEntity>(configuration.ContainerName, id, partitionKey, cancellationToken);

        if (entity is null)
            return default;

        ChangeTracker.RegisterUnchanged(entity);

        return entity;
    }

    public IQueryable<TEntity> Query<TEntity>(string partitionKey)
    {
        var entityConfiguration = DocumentStore.GetConfiguration<TEntity>();
        var queryable = Database.CreateLinqQuery<TEntity>(entityConfiguration.ContainerName);

        return new CosmodustLinqQuery<TEntity>(
            Database,
            ChangeTracker,
            entityConfiguration,
            partitionKey,
            queryable);
    }

    public SqlQuery<TEntity> Query<TEntity>(string sql, string partitionKey)
    {
        var config = DocumentStore.GetConfiguration<TEntity>();

        return new SqlQuery<TEntity>(
            database: Database,
            changeTracker: ChangeTracker,
            entityConfiguration: config,
            sql: sql,
            partitionKey: partitionKey);
    }

    public void Store<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterAdded(entity);
    }

    public void Update<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterModified(entity);
    }

    public void Remove<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ChangeTracker.RegisterRemoved(entity);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);
        ChangeTracker.Commit();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);
        ChangeTracker.Commit();
    }
}
