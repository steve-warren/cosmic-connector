using System.Runtime.CompilerServices;
using CosmicConnector.Linq;

namespace CosmicConnector;

public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(DocumentStore documentStore, EntityConfigurationHolder entityConfiguration, IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(documentStore);
        ArgumentNullException.ThrowIfNull(entityConfiguration);

        DocumentStore = documentStore;
        Database = database;
        EntityConfiguration = entityConfiguration;

        IdentityMap = new IdentityMap(entityConfiguration);
        ChangeTracker = new ChangeTracker(entityConfiguration);
    }

    public ChangeTracker ChangeTracker { get; }
    public IdentityMap IdentityMap { get; }
    public DocumentStore DocumentStore { get; }
    public IDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        if (IdentityMap.TryGet(id, out TEntity? entity))
            return entity;

        entity = await Database.FindAsync<TEntity>(id, partitionKey, cancellationToken);

        if (entity is null)
            return default;

        IdentityMap.Attach(id, entity);
        ChangeTracker.RegisterUnchanged(entity);

        return entity;
    }

    public IQueryable<TEntity> Query<TEntity>(string? partitionKey = null)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        return new DocumentQuery<TEntity>(this, Database.GetLinqQuery<TEntity>(partitionKey));
    }

    public async IAsyncEnumerable<TEntity> GetAsyncEnumerable<TEntity>(IQueryable<TEntity> queryable, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(queryable);

        var query = Database.GetAsyncEnumerable(queryable, cancellationToken);

        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            if (entity is null)
                continue;

            IdentityMap.Attach(entity);
            ChangeTracker.RegisterUnchanged(entity);

            yield return entity;
        }
    }

    public void Store<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        IdentityMap.Attach(entity);
        ChangeTracker.RegisterAdded(entity);
    }

    public void Update<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        IdentityMap.EnsureExists(entity);
        ChangeTracker.RegisterModified(entity);
    }

    public void Remove<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        IdentityMap.EnsureExists(entity);
        ChangeTracker.RegisterRemoved(entity);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);

        foreach (var entry in ChangeTracker.RemovedEntries)
            IdentityMap.Detatch(entry.EntityType, entry.Id);

        ChangeTracker.Commit();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);

        foreach (var entry in ChangeTracker.RemovedEntries)
            IdentityMap.Detatch(entry.EntityType, entry.Id);

        ChangeTracker.Commit();
    }
}
