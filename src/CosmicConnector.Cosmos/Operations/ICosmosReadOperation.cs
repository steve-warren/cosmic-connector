namespace CosmicConnector.Cosmos;

internal interface ICosmosReadOperation<TEntity> where TEntity : class
{
    Task<TEntity?> ExecuteAsync(CancellationToken cancellationToken = default);
}
