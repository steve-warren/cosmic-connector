using Cosmodust.Query;
using Cosmodust.Tracking;

namespace Cosmodust.Session;

public interface IDocumentSession
{
    ChangeTracker ChangeTracker { get; }

    ValueTask<TEntity?> FindAsync<TEntity>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default);
    void Store<TEntity>(TEntity entity);
    void Remove<TEntity>(TEntity entity);
    void Update<TEntity>(TEntity entity);
    IQueryable<TEntity> Query<TEntity>(string partitionKey);
    SqlQuery<TEntity> Query<TEntity>(
        string partitionKey,
        string sql,
        object? parameters = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}
