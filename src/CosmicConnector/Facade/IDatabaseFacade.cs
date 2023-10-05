namespace CosmicConnector;

public interface IDatabaseFacade
{
    ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class;
}
