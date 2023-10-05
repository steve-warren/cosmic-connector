namespace CosmicConnector;

public sealed class DocumentSession : IDocumentSession
{
    private readonly DocumentStore _documentStore;
    private readonly IdentityMap _identityMap = new();

    public DocumentSession(DocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        _identityMap.TryGet(id, out TEntity? entity);
        return entity;
    }

    public IQueryable<TEntity> Query<TEntity>()
    {
        throw new NotImplementedException();
    }

    public void Store<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _identityMap.Put("id", entity);
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
