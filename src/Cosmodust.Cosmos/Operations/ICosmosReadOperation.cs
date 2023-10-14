namespace Cosmodust.Cosmos;

internal interface ICosmosReadOperation<TResult>
{
    Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
