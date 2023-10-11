namespace CosmoDust.Cosmos;

internal interface ICosmosReadOperation<TResult>
{
    Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
