namespace CosmicConnector;

public interface IDatabaseFacade
{
    EntityConfigurationHolder EntityConfiguration { get; set; }
    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class;
    IQueryable<TEntity> GetLinqQuery<TEntity>() where TEntity : class;
    IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(IQueryable<TEntity> queryable) where TEntity : class;
    Task<TEntity?> FirstOrDefaultAsync<TEntity>(IQueryable<TEntity> queryable, CancellationToken cancellationToken = default) where TEntity : class;
    Task CommitAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default);
}
