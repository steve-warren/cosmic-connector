namespace CosmicConnector;

public interface IDocumentSession
{
    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class;
    void Store(object entity);
    void Delete(object entity);
    IQueryable<TEntity> Query<TEntity>();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
