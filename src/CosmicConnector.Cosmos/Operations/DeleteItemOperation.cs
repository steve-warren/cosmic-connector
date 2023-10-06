using Microsoft.Azure.Cosmos;

namespace CosmicConnector.Cosmos;

internal class DeleteItemOperation : ICosmosWriteOperation
{
    private readonly Container _container;
    private readonly EntityEntry _entityEntry;

    public DeleteItemOperation(Container container, EntityEntry entry)
    {
        _container = container;
        _entityEntry = entry;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _container.DeleteItemStreamAsync(_entityEntry.Id, new PartitionKey(_entityEntry.Id), cancellationToken: cancellationToken);
    }
}
