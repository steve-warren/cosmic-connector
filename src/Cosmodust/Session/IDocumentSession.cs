using Cosmodust.Linq;

namespace Cosmodust;

public interface IDocumentSession
{
    ChangeTracker ChangeTracker { get; }

    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default);
    void Store<TEntity>(TEntity entity);
    void Remove<TEntity>(TEntity entity);
    void Update<TEntity>(TEntity entity);
    IQueryable<TEntity> Query<TEntity>(string? partitionKey = null);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}
