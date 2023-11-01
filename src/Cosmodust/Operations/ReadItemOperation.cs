using System.Diagnostics;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal class ReadItemOperation<TEntity>
{
    private readonly Container _container;
    private readonly string _id;
    private readonly string _partitionKey;

    public ReadItemOperation(Container container, string id, string partitionKey)
    {
        _container = container;
        _id = id;
        _partitionKey = partitionKey;
    }

    public async ValueTask<ReadOperationResult<TEntity?>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<TEntity>(
                _id,
                new PartitionKey(_partitionKey),
                cancellationToken: cancellationToken);

            Debug.WriteLine(
                $"Transaction operation HTTP {response.StatusCode} - RUs {response.Headers.RequestCharge}");

            return new ReadOperationResult<TEntity?>(
                response.Resource,
                response.StatusCode,
                response.ETag,
                response.RequestCharge);
        }

        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return new ReadOperationResult<TEntity?>(default(TEntity), ex.StatusCode);
        }
    }
}
