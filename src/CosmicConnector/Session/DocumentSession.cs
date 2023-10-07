namespace CosmicConnector;

public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(DocumentStore documentStore, IdentityAccessor identityAccessor, IDatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(documentStore);
        ArgumentNullException.ThrowIfNull(identityAccessor);

        DocumentStore = documentStore;
        DatabaseFacade = databaseFacade;

        IdentityMap = new IdentityMap(identityAccessor);
        ChangeTracker = new ChangeTracker(identityAccessor);
    }

    public ChangeTracker ChangeTracker { get; }
    public IdentityMap IdentityMap { get; }
    public DocumentStore DocumentStore { get; }
    public IDatabaseFacade DatabaseFacade { get; }

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        if (IdentityMap.TryGet(id, out TEntity? entity))
            return entity;

        entity = await DatabaseFacade.FindAsync<TEntity>(id, partitionKey, cancellationToken);

        IdentityMap.Attach(id, entity);

        if (entity is not null)
            ChangeTracker.RegisterUnchanged(entity);

        return entity;
    }

    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        DocumentStore.EnsureConfigured<TEntity>();
        return DatabaseFacade.Query<TEntity>();
    }

    public void Store<TEntity>(TEntity entity) where TEntity : class
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        IdentityMap.Attach(entity);
        ChangeTracker.RegisterAdded(entity);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        IdentityMap.EnsureExists(entity);
        ChangeTracker.RegisterModified(entity);
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        IdentityMap.EnsureExists(entity);
        ChangeTracker.RegisterRemoved(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DatabaseFacade.SaveChangesAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);

        foreach (var entry in ChangeTracker.RemovedEntries)
            IdentityMap.Detatch(entry.EntityType, entry.Id);

        ChangeTracker.Commit();
    }
}
