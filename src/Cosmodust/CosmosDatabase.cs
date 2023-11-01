using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cosmodust.Linq;
using Cosmodust.Operations;
using Cosmodust.Query;
using Cosmodust.Shared;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Cosmodust;

/// <summary>
/// Represents a Cosmos DB database.
/// </summary>
public sealed class CosmosDatabase : IDatabase
{
    private readonly Dictionary<string, Container> _containers = new();
    private readonly Database _database;
    private readonly CosmosLinqSerializerOptions _cosmosLinqSerializerOptions;

    public CosmosDatabase(Database database, CosmosLinqSerializerOptions cosmosLinqSerializerOptions)
    {
        _database = database;
        _cosmosLinqSerializerOptions = cosmosLinqSerializerOptions;
    }

    public string Name => _database.Id;

    /// <summary>
    /// Finds an entity of type <typeparamref name="TEntity"/> in the specified container with the given ID and partition key.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to find.</typeparam>
    /// <param name="containerName">The name of the container to search for the entity.</param>
    /// <param name="id">The ID of the entity to find.</param>
    /// <param name="partitionKey">The partition key of the entity to find.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    public ValueTask<OperationResult> FindAsync<TEntity>(
        string containerName,
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrWhiteSpace(containerName);
        Ensure.NotNullOrWhiteSpace(id);
        Ensure.NotNullOrWhiteSpace(partitionKey);

        var container = GetContainerFor(containerName);

        var operation = new ReadItemOperation<TEntity?>(container, id, partitionKey);

        return operation.ExecuteAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a LINQ queryable for the specified Cosmos DB container.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query.</typeparam>
    /// <param name="containerName">The name of the Cosmos DB container.</param>
    /// <returns>A LINQ queryable for the specified Cosmos DB container.</returns>
    public IQueryable<TEntity> CreateLinqQuery<TEntity>(string containerName)
    {
        Ensure.NotNullOrWhiteSpace(containerName);

        return GetContainerFor(containerName).GetItemLinqQueryable<TEntity>(
            linqSerializerOptions: _cosmosLinqSerializerOptions);
    }

    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(
        CosmodustLinqQuery<TEntity> query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(query);

        var originalQueryDefinition = query.DatabaseLinqQuery.ToQueryDefinition();
        var typedQuerySql = originalQueryDefinition.QueryText + " AND root.__type = @type";
        var typedQueryDefinition = new QueryDefinition(query: typedQuerySql);
        typedQueryDefinition.WithParameter("@type", typeof(TEntity).Name);

        var container = GetContainerFor(query.EntityConfiguration.ContainerName);

        var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(query.PartitionKey) };

        using var feed = container.GetItemQueryIterator<TEntity>(
            typedQueryDefinition,
            continuationToken: null,
            queryRequestOptions);

        while (feed.HasMoreResults)
        {
            var response = await feed
                .ReadNextAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var entity in response)
                yield return entity;
        }
    }

    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(
        SqlQuery<TEntity> query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(query);

        var queryDefinition = new QueryDefinition(query.Sql);

        foreach (var parameter in query.Parameters)
            queryDefinition.WithParameter(name: parameter.Name, value: parameter.Value);

        var container = GetContainerFor(query.EntityConfiguration.ContainerName);

        var queryRequestOptions =
            new QueryRequestOptions { PartitionKey = new PartitionKey(query.PartitionKey) };

        using var feed = container.GetItemQueryIterator<TEntity>(
            queryDefinition,
            continuationToken: null,
            queryRequestOptions);

        while (feed.HasMoreResults)
        {
            var response = await feed
                .ReadNextAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var entity in response)
                yield return entity;
        }
    }

    public async Task CommitAsync(
        IEnumerable<EntityEntry> entries,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(entries);

        foreach (var entry in entries)
        {
            if (entry.IsUnchanged)
                continue;

            entry.SendJsonPropertiesToSerializer();

            var operation = CreateWriteOperation(entry);
            var response = await operation.ExecuteAsync(cancellationToken);

            Debug.WriteLine(
                $"Write operation HTTP {response.StatusCode} - {response.Cost} RUs");
        }
    }

    public async Task CommitTransactionAsync(
        IEnumerable<EntityEntry> entries,
        CancellationToken cancellationToken)
    {
        Ensure.NotNull(entries);

        var batchOperation = new TransactionalBatchOperation(
            database: _database,
            entries: entries);

        await batchOperation.ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private ICosmosWriteOperation CreateWriteOperation(EntityEntry entry)
    {
        var container = GetContainerFor(entry.ContainerName);

        return entry.State switch
        {
            EntityState.Added => new CreateItemOperation(container, entry.Entity, entry.PartitionKey),
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
