using Microsoft.Azure.Cosmos;

namespace CosmicConnector.Cosmos;

public sealed class CosmosDatabaseFacade : IDatabaseFacade
{
    private readonly CosmosClient _client;
    private readonly Dictionary<Type, Container> _containers = new();

    public CosmosDatabaseFacade(CosmosClient client)
    {
        _client = client;
    }

    public EntityConfigurationHolder EntityConfiguration { get; set; } = new();

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        var container = GetContainerFor(typeof(TEntity));

        var operation = new ReadItemOperation<TEntity>(container, id, partitionKey);

        var entity = await operation.ExecuteAsync(cancellationToken);

        return entity;
    }

    public IQueryable<TEntity> GetLinqQuery<TEntity>() where TEntity : class
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TEntity> ExecuteQuery<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public async Task CommitAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            if (entry.IsUnchanged)
                return;

            var operation = CreateOperation(entry);
            await operation.ExecuteAsync(cancellationToken);
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

        container = _client.GetContainer(entityConfiguration.DatabaseName, entityConfiguration.ContainerName);

        _containers.Add(entityType, container);

        return container;
    }

    private EntityConfiguration GetConfigurationFor(Type entityType) => EntityConfiguration.Get(entityType) ?? throw new InvalidOperationException("No configuration found for the given entity type.");
}
