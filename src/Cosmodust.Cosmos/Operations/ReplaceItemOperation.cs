using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos;

internal class ReplaceItemOperation : ICosmosWriteOperation
{
    private readonly Container _container;
    private readonly object _entity;
    private readonly string _id;
    private readonly string? _partitionKey;

    public ReplaceItemOperation(Container container, object entity, string id, string? partitionKey)
    {
        _container = container;
        _entity = entity;
        _id = id;
        _partitionKey = string.IsNullOrEmpty(partitionKey) ? id : partitionKey;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _container.ReplaceItemAsync(_entity, _id, new PartitionKey(_partitionKey), cancellationToken: cancellationToken);
    }
}
