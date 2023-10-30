using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal class CreateItemOperation : ICosmosWriteOperation
{
    private static readonly ItemRequestOptions s_defaultItemRequestOptions = new ItemRequestOptions
        { EnableContentResponseOnWrite = false };

    private readonly Container _container;
    private readonly EntityEntry _entityEntry;

    public CreateItemOperation(
        Container container,
        EntityEntry entityEntry)
    {
        _container = container;
        _entityEntry = entityEntry;
    }

    public Task<ItemResponse<object>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _container.CreateItemAsync(
            item: _entityEntry.Entity,
            partitionKey: new PartitionKey(_entityEntry.PartitionKey),
            requestOptions: s_defaultItemRequestOptions,
            cancellationToken: cancellationToken);
    }
}
