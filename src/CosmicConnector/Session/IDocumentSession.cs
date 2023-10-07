namespace CosmicConnector;

public interface IDocumentSession
{
    ChangeTracker ChangeTracker { get; }
    IdentityMap IdentityMap { get; }

    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class;
    void Store<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
