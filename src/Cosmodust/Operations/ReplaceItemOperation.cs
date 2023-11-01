using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal class ReplaceItemOperation : ICosmosWriteOperation
{
    private static readonly ItemRequestOptions s_defaultItemRequestOptions = new()
        { EnableContentResponseOnWrite = false };
    private readonly Container _container;
    private readonly EntityEntry _entityEntry;

    public ReplaceItemOperation(Container container, EntityEntry entityEntry)
    {
        _container = container;
        _entityEntry = entityEntry;
    }

    public async Task<ItemResponse<object>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var response = await _container.ReplaceItemAsync(
            item: _entityEntry.Entity,
            id: _entityEntry.Id,
            partitionKey: new PartitionKey(_entityEntry.PartitionKey),
            requestOptions: s_defaultItemRequestOptions,
            cancellationToken: cancellationToken);

        _entityEntry.Modify(response.ETag);

        return response;
    }
}
