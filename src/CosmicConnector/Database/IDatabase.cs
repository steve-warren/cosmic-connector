namespace CosmicConnector;

public interface IDatabase
{
    EntityConfigurationHolder EntityConfiguration { get; set; }
    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default);
    IQueryable<TEntity> GetLinqQuery<TEntity>(string? partitionKey = null);
    IAsyncEnumerable<TEntity> GetAsyncEnumerable<TEntity>(IQueryable<TEntity> queryable, CancellationToken cancellationToken = default);
    Task CommitAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default);
}
