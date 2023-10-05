namespace CosmicConnector;

public sealed class DocumentSession : IDocumentSession
{
    private readonly DocumentStore _documentStore;
    private readonly IdentityMap _identityMap = new();

    internal DocumentSession(DocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        _documentStore.IdAccessor.EnsureRegistered<TEntity>();

        _identityMap.TryGet(id, out TEntity? entity);

        return ValueTask.FromResult(entity);
    }

    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        throw new NotImplementedException();
    }

    public void Store<TEntity>(TEntity entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = _documentStore.IdAccessor.GetEntityId(entity);

        _identityMap.Put(id, entity);
    }

    public void Update(object entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(object entity)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
