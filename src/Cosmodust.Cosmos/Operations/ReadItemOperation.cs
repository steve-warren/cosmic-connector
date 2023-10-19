using System.Net;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos.Operations;

internal class ReadItemOperation<TResult> : ICosmosReadOperation<TResult?>
{
    private readonly Container _container;
    private readonly string _id;
    private readonly string? _partitionKey;

    public ReadItemOperation(Container container, string id, string? partitionKey)
    {
        _container = container;
        _id = id;
        _partitionKey = partitionKey;
    }

    public async Task<TResult?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<TResult>(_id, new PartitionKey(_partitionKey ?? _id), cancellationToken: cancellationToken);

            return response.Resource;
        }

        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }
}
