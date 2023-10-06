namespace CosmicConnector;

public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(IdentityAccessor identityAccessor, IDatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(identityAccessor);

        IdentityAccessor = identityAccessor;
        DatabaseFacade = databaseFacade;
    }

    public ChangeTracker ChangeTracker { get; } = new();
    public IdentityMap IdentityMap { get; } = new();
    public IdentityAccessor IdentityAccessor { get; }
    public IDatabaseFacade DatabaseFacade { get; }

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        IdentityAccessor.EnsureRegistered<TEntity>();

        if (!IdentityMap.TryGet(id, out TEntity? entity))
        {
            entity = await DatabaseFacade.FindAsync<TEntity>(id, partitionKey, cancellationToken);

            IdentityMap.Put(id, entity);

            if (entity is not null)
                ChangeTracker.TrackUnchanged(id, entity);
        }

        return entity;
    }

    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        throw new NotImplementedException();
    }

    public void Store<TEntity>(TEntity entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = IdentityAccessor.GetId(entity);

        IdentityMap.Put(id, entity);
        ChangeTracker.TrackAdded(id, entity);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = ChangeTracker.FindEntry(entity) ?? throw new InvalidOperationException($"Cannot update entity of type {typeof(TEntity)} because it has not been loaded into the session.");

        entry.Modify();
    }

    public void Remove(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = ChangeTracker.FindEntry(entity) ?? throw new InvalidOperationException($"Cannot remove entity of type {entity.GetType()} because it has not been loaded into the session.");

        entry.Remove();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DatabaseFacade.SaveChangesAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);

        foreach (var entry in ChangeTracker.RemovedEntries)
            IdentityMap.Detatch(entry.EntityType, entry.Id);

        ChangeTracker.Reset();
    }
}
