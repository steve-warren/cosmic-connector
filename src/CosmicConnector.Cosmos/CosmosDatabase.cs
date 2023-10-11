using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CosmicConnector.Cosmos;

public sealed class CosmosDatabase : IDatabase
{
    private static readonly CosmosLinqSerializerOptions s_linqSerializerOptions = new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };
    private static readonly QueryRequestOptions s_defaultQueryRequestOptions = new();

    private readonly Dictionary<Type, Container> _containers = new();

    public CosmosDatabase(Database database)
    {
        Database = database;
    }

    public string Name => Database.Id;
    public Database Database { get; }

    public EntityConfigurationHolder EntityConfiguration { get; set; } = new();

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var container = GetContainerFor(typeof(TEntity));

        var operation = new ReadItemOperation<TEntity>(container, id, partitionKey);

        var entity = await operation.ExecuteAsync(cancellationToken);

        return entity;
    }

    public IQueryable<TEntity> GetLinqQuery<TEntity>(string? partitionKey = null)
    {
        var container = GetContainerFor(typeof(TEntity));

        var queryRequestOptions = partitionKey is null ? s_defaultQueryRequestOptions : new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) };

        var query = container.GetItemLinqQueryable<TEntity>(requestOptions: queryRequestOptions, linqSerializerOptions: s_linqSerializerOptions);

        return query;
    }

    public async IAsyncEnumerable<TEntity> GetAsyncEnumerable<TEntity>(IQueryable<TEntity> queryable, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var feed = queryable.ToFeedIterator();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken);

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
            await operation.ExecuteAsync(cancellationToken);
        }
    }

    public async Task CommitTransactionAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken)
    {
        var containerAndPartitionKey = entries.GroupBy(e => (e.ContainerName, e.PartitionKey));

        foreach (var entriesGrouping in containerAndPartitionKey)
        {
            var container = Database.GetContainer(entriesGrouping.Key.ContainerName);
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
        }
    }

    private ICosmosWriteOperation CreateOperation(EntityEntry entry)
    {
        var container = GetContainerFor(entry.EntityType);

        return entry.State switch
        {
            EntityState.Added => new CreateItemOperation(container, entry.Entity),
            EntityState.Removed => new DeleteItemOperation(container, entry.Id, entry.PartitionKey),
            EntityState.Modified => new ReplaceItemOperation(container, entry.Entity, entry.Id, entry.PartitionKey),
            _ => throw new NotImplementedException()
        };
    }

    private Container GetContainerFor(Type entityType)
    {
        if (_containers.TryGetValue(entityType, out var container))
            return container;

        var entityConfiguration = GetConfigurationFor(entityType);

        container = Database.GetContainer(entityConfiguration.ContainerName);

        _containers.Add(entityType, container);

        return container;
    }

    private EntityConfiguration GetConfigurationFor(Type entityType) => EntityConfiguration.Get(entityType) ?? throw new InvalidOperationException("No configuration found for the given entity type.");
}
