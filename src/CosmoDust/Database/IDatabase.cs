using CosmoDust.Linq;

namespace CosmoDust;

public interface IDatabase
{
    string Name { get; }
    ValueTask<TEntity?> FindAsync<TEntity>(string containerName, string id, string? partitionKey = default, CancellationToken cancellationToken = default);
    IQueryable<TEntity> GetLinqQuery<TEntity>(string containerName, string? partitionKey = null);
    IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(CosmodustLinqQuery<TEntity> queryable, CancellationToken cancellationToken = default);
    Task CommitAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default);
}
