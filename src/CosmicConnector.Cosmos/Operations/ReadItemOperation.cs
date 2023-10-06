using System.Net;
using Microsoft.Azure.Cosmos;

namespace CosmicConnector.Cosmos;

internal class ReadItemOperation<TEntity> : ICosmosReadOperation<TEntity>
    where TEntity : class
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

    public async Task<TEntity?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<TEntity>(_id, new PartitionKey(_partitionKey ?? _id), cancellationToken: cancellationToken);

            return response.Resource;
        }

        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }
}
