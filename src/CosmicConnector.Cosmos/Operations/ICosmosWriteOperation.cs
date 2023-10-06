namespace CosmicConnector.Cosmos;

internal interface ICosmosWriteOperation
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
