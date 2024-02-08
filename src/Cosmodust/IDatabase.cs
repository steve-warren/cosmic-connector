using System.Runtime.CompilerServices;
using Cosmodust.Linq;
using Cosmodust.Operations;
using Cosmodust.Query;
using Cosmodust.Tracking;

namespace Cosmodust;

public interface IDatabase
{
    string Name { get; }
    ValueTask<OperationResult> FindAsync<TEntity>(
        string containerName,
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default);
    IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(
        CosmodustLinqQuery<TEntity> query,
        CancellationToken cancellationToken = default);
    IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(
        SqlQuery<TEntity> query,
        CancellationToken cancellationToken = default);
    Task<OperationResult> CommitAsync(
        EntityEntry entry,
        CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(
        IEnumerable<EntityEntry> entries,
        CancellationToken cancellationToken = default);
    IQueryable<TEntity> CreateLinqQuery<TEntity>(string containerName);
}
