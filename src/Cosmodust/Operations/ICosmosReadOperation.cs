namespace Cosmodust.Operations;

internal interface ICosmosReadOperation<TResult>
{
    Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
