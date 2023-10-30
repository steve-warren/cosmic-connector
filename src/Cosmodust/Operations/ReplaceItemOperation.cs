using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos.Operations;

internal class ReplaceItemOperation : ICosmosWriteOperation
{
    private static readonly ItemRequestOptions s_defaultItemRequestOptions = new ItemRequestOptions
        { EnableContentResponseOnWrite = false };
    private readonly Container _container;
    private readonly EntityEntry _entityEntry;

    public ReplaceItemOperation(Container container, EntityEntry entityEntry)
    {
        _container = container;
        _entityEntry = entityEntry;
    }

    public Task<ItemResponse<object>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _container.ReplaceItemAsync(
            item: _entityEntry.Entity,
            id: _entityEntry.Id,
            partitionKey: new PartitionKey(_entityEntry.PartitionKey),
            requestOptions: s_defaultItemRequestOptions,
            cancellationToken: cancellationToken);
    }
}
