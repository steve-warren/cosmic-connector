using Cosmodust.Linq;

namespace Cosmodust;

public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(DocumentStore documentStore, EntityConfigurationHolder entityConfiguration, IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(documentStore);
        ArgumentNullException.ThrowIfNull(entityConfiguration);

        DocumentStore = documentStore;
        Database = database;
        EntityConfiguration = entityConfiguration;

        ChangeTracker = new ChangeTracker(entityConfiguration);
    }

    public ChangeTracker ChangeTracker { get; }
    public DocumentStore DocumentStore { get; }
    public IDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var configuration = DocumentStore.GetConfiguration(typeof(TEntity));
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        if (ChangeTracker.TryGet(id, out TEntity? entity))
            return entity;

        entity = await Database.FindAsync<TEntity>(configuration.ContainerName, id, partitionKey, cancellationToken);

        if (entity is null)
            return default;

        ChangeTracker.RegisterUnchanged(entity);

        return entity;
    }

    public IQueryable<TEntity> Query<TEntity>(string? partitionKey = null)
    {
        var config = DocumentStore.GetConfiguration(typeof(TEntity));
        var queryable = Database.GetLinqQuery<TEntity>(config.ContainerName, partitionKey);

        return new CosmodustLinqQuery<TEntity>(Database, ChangeTracker, queryable);
    }

    public void Store<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        ChangeTracker.RegisterAdded(entity);
    }

    public void Update<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        ChangeTracker.EnsureExists(entity);
        ChangeTracker.RegisterModified(entity);
    }

    public void Remove<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        ChangeTracker.EnsureExists(entity);
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
