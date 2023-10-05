namespace CosmicConnector;

public interface IDocumentSession
{
    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class;
    void Store<TEntity>(TEntity entity) where TEntity : class;
    void Delete(object entity);
    void Update(object entity);
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
