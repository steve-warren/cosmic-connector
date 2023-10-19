using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos.Operations;

internal class DeleteItemOperation : ICosmosWriteOperation
{
    private readonly Container _container;
    private readonly string _id;
    private readonly string? _partitionKey;

    public DeleteItemOperation(Container container, string id, string? partitionKey)
    {
        _container = container;
        _id = id;
        _partitionKey = string.IsNullOrEmpty(partitionKey) ? id : partitionKey;
    }

    public Task<ItemResponse<object>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _container.DeleteItemAsync<object>(_id, new PartitionKey(_partitionKey), cancellationToken: cancellationToken);
    }
}
