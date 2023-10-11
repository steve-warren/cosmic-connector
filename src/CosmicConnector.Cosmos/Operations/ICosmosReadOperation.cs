namespace CosmicConnector.Cosmos;

internal interface ICosmosReadOperation<TResult>
{
    Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
