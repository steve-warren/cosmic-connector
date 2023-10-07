namespace CosmicConnector.Cosmos;

internal interface ICosmosReadOperation<TEntity>
{
    Task<TEntity?> ExecuteAsync(CancellationToken cancellationToken = default);
}
