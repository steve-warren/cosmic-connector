namespace CosmicConnector;

public sealed class DocumentSession : IDocumentSession
{
    private readonly DocumentStore _documentStore;

    public DocumentSession(DocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> Query<TEntity>()
    {
        throw new NotImplementedException();
    }

    public void Store(object entity)
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
