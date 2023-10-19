namespace Cosmodust.Cosmos.Operations;

internal interface ICosmosWriteOperation
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
