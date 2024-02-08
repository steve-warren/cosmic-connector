using Cosmodust.Shared;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal class DeleteItemOperation : IDocumentWriteOperation
{
    private static readonly ItemRequestOptions s_requestOptions = new()
        { EnableContentResponseOnWrite = false };

    private readonly Container _container;
    private readonly string _id;
    private readonly string _partitionKey;

    public DeleteItemOperation(
        Container container,
        string id,
        string partitionKey)
    {
        Ensure.NotNull(container);
        Ensure.NotNullOrWhiteSpace(id);
        Ensure.NotNullOrWhiteSpace(partitionKey);

        _container = container;
        _id = id;
        _partitionKey = partitionKey;
    }

    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var response = await _container.DeleteItemAsync<object>(
            _id,
            new PartitionKey(_partitionKey),
            requestOptions: s_requestOptions,
            cancellationToken: cancellationToken);
        
        return response.ToOperationResult();
    }
}
