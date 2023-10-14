namespace Cosmodust.Cosmos;

internal interface ICosmosWriteOperation
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
