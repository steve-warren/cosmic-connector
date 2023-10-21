using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;
using System.Diagnostics;
using Cosmodust.Cosmos.Operations;
using Cosmodust.Linq;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos.Linq;

namespace Cosmodust.Cosmos;

public sealed class CosmosDatabase : IDatabase
{
    private static readonly QueryRequestOptions s_defaultQueryRequestOptions = new();
    private static readonly CosmosLinqSerializerOptions s_cosmosLinqSerializerOptions =
        new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };

    private readonly Dictionary<string, Container> _containers = new();
    private readonly Database _database;

    public CosmosDatabase(Database database)
    {
        _database = database;
    }

    public string Name => _database.Id;

    public async ValueTask<TEntity?> FindAsync<TEntity>(
        string containerName,
        string id,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        var container = GetContainerFor(containerName);

        var operation = new ReadItemOperation<TEntity>(container, id, partitionKey);

        var entity = await operation.ExecuteAsync(cancellationToken);

        return entity;
    }

    public IQueryable<TEntity> CreateLinqQuery<TEntity>(string containerName) =>
        GetContainerFor(containerName).GetItemLinqQueryable<TEntity>(
            linqSerializerOptions: s_cosmosLinqSerializerOptions);

    public IQueryable<TEntity> GetLinqQuery<TEntity>(string containerName, string? partitionKey = null)
    {
        var container = GetContainerFor(containerName);

        var queryRequestOptions = partitionKey is null
            ? s_defaultQueryRequestOptions
            : new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) };

        var query = container.GetItemLinqQueryable<TEntity>(
            requestOptions: queryRequestOptions,
            linqSerializerOptions: s_cosmosLinqSerializerOptions);

        return query;
    }

    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(
        CosmodustLinqQuery<TEntity> query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var originalQueryDefinition = query.CosmosLinqQuery.ToQueryDefinition();
        var typedQuerySql = originalQueryDefinition.QueryText + " AND root.__type = @type";
        var typedQueryDefinition = new QueryDefinition(query: typedQuerySql);
        typedQueryDefinition.WithParameter("@type", typeof(TEntity).Name);
        
        var container = GetContainerFor(query.EntityConfiguration.ContainerName);
        
        var queryRequestOptions =
            query.PartitionKey is null
            ? s_defaultQueryRequestOptions
            : new QueryRequestOptions { PartitionKey = new PartitionKey(query.PartitionKey) };

        using var feed = container.GetItemQueryIterator<TEntity>(
            typedQueryDefinition,
            continuationToken: null,
            queryRequestOptions);
 
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken).ConfigureAwait(false);

            foreach (var entity in response)
                yield return entity;
        }
    }

    public async Task CommitAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            if (entry.IsUnchanged)
                continue;

            var operation = CreateOperation(entry);
            var response = await operation.ExecuteAsync(cancellationToken);

            Debug.WriteLine(response.StatusCode);
        }
    }

    public async Task CommitTransactionAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken)
    {
        var containerAndPartitionKey = entries.GroupBy(e => (e.ContainerName, e.PartitionKey));

        foreach (var entriesGrouping in containerAndPartitionKey)
        {
            var container = _database.GetContainer(entriesGrouping.Key.ContainerName);
            var partitionKey = entriesGrouping.Key.PartitionKey;

            var batch = container.CreateTransactionalBatch(new PartitionKey(partitionKey));

            foreach (var entry in entriesGrouping)
            {
                _ = entry.State switch
                {
                    EntityState.Added => batch.CreateItem(entry.Entity),
                    EntityState.Removed => batch.DeleteItem(entry.Id),
                    EntityState.Modified => batch.ReplaceItem(entry.Id, entry.Entity),
                    EntityState.Unchanged or
                    EntityState.Detached => batch,
                    _ => throw new NotImplementedException()
                };
            }

            var response = await batch.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            Debug.WriteLine("Request charge: {0}", response.RequestCharge);
        }
    }

    private ICosmosWriteOperation CreateOperation(EntityEntry entry)
    {
        var container = GetContainerFor(entry.ContainerName);

        return entry.State switch
        {
            EntityState.Added => new CreateItemOperation(container, entry.Entity),
            EntityState.Removed => new DeleteItemOperation(container, entry.Id, entry.PartitionKey),
            EntityState.Modified => new ReplaceItemOperation(container, entry.Entity, entry.Id, entry.PartitionKey),
            _ => throw new NotImplementedException()
        };
    }

    private Container GetContainerFor(string containerName)
    {
        if (_containers.TryGetValue(containerName, out var container))
            return container;

        container = _database.GetContainer(containerName);

        _containers.Add(key: containerName, value: container);

        return container;
    }
}
