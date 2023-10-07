using CosmicConnector.Linq;

namespace CosmicConnector;

public interface IDocumentSession
{
    ChangeTracker ChangeTracker { get; }
    IdentityMap IdentityMap { get; }

    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default);
    void Store<TEntity>(TEntity entity);
    void Remove<TEntity>(TEntity entity);
    void Update<TEntity>(TEntity entity);
    IQueryable<TEntity> Query<TEntity>(string? partitionKey = null);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
