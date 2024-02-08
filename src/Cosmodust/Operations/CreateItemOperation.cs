using Cosmodust.Shared;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal class CreateItemOperation : IDocumentWriteOperation
{
    private static readonly ItemRequestOptions s_requestOptions = new()
    { EnableContentResponseOnWrite = false };

    private readonly Container _container;
    private readonly object _entity;
    private readonly string _partitionKey;

    public CreateItemOperation(
        Container container,
        object entity,
        string partitionKey)
    {
        Ensure.NotNull(container);
        Ensure.NotNull(entity);
        Ensure.NotNullOrWhiteSpace(partitionKey);

        _container = container;
        _entity = entity;
        _partitionKey = partitionKey;
    }

    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var response = await _container.CreateItemAsync(
            item: _entity,
            partitionKey: new PartitionKey(_partitionKey),
            requestOptions: s_requestOptions,
            cancellationToken: cancellationToken);

        return response.ToOperationResult();
    }
}
