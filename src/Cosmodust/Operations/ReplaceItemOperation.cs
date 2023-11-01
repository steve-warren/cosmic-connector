using Cosmodust.Shared;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal class ReplaceItemOperation : ICosmosWriteOperation
{
    private static readonly ItemRequestOptions s_requestOptions = new()
        { EnableContentResponseOnWrite = false };

    private readonly Container _container;
    private readonly object _entity;
    private readonly string _id;
    private readonly string _partitionKey;

    public ReplaceItemOperation(
        Container container,
        object entity,
        string id,
        string partitionKey)
    {
        Ensure.NotNull(container);
        Ensure.NotNull(entity);
        Ensure.NotNullOrWhiteSpace(id);
        Ensure.NotNullOrWhiteSpace(partitionKey);

        _container = container;
        _entity = entity;
        _id = id;
        _partitionKey = partitionKey;
    }

    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var response = await _container.ReplaceItemAsync(
            item: _entity,
            id: _id,
            partitionKey: new PartitionKey(_partitionKey),
            requestOptions: s_requestOptions,
            cancellationToken: cancellationToken);

        return response.ToOperationResult();
    }
}
