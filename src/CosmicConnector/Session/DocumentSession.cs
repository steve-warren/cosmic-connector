namespace CosmicConnector;

public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(IdentityAccessor identityAccessor)
    {
        ArgumentNullException.ThrowIfNull(identityAccessor);

        IdentityAccessor = identityAccessor;
    }

    public ChangeTracker ChangeTracker { get; } = new();
    public IdentityMap IdentityMap { get; } = new();
    public IdentityAccessor IdentityAccessor { get; }

    public ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        IdentityAccessor.EnsureRegistered<TEntity>();

        IdentityMap.TryGet(id, out TEntity? entity);

        return ValueTask.FromResult(entity);
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
        ChangeTracker.Track(id, entity);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = ChangeTracker.FindEntry(entity) ?? throw new InvalidOperationException($"Cannot update entity of type {typeof(TEntity)} because it has not been loaded into the session.");

        entry.Modify();
    }

    public void Delete(object entity)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entity in ChangeTracker.Entries)
            entity.Unchange();

        return Task.CompletedTask;
    }
}
