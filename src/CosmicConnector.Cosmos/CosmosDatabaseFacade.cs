using Microsoft.Azure.Cosmos;

namespace CosmicConnector.Cosmos;

public class CosmosDatabaseFacade : IDatabaseFacade
{
    private readonly CosmosClient _client;
    private readonly Dictionary<Type, Container> _containers = new();

    public CosmosDatabaseFacade(string connectionString)
    {
        _client = new CosmosClient(connectionString);
    }

    public EntityConfigurationHolder EntityConfiguration { get; set; } = new();

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        var container = GetContainerFor<TEntity>();

        var response = await container.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey ?? id), cancellationToken: cancellationToken);

        return response.Resource;
    }

    public Task SaveChangesAsync(EntityEntry entry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private Container GetContainerFor<TEntity>()
    {
        if (_containers.TryGetValue(typeof(TEntity), out var container))
            return container;

        var entityConfiguration = GetConfigurationFor<TEntity>();

        container = _client.GetContainer(entityConfiguration.DatabaseName, entityConfiguration.ContainerName);

        _containers.Add(typeof(TEntity), container);

        return container;
    }

    private EntityConfiguration GetConfigurationFor<TEntity>() => EntityConfiguration.Get(typeof(TEntity)) ?? throw new InvalidOperationException("No configuration found for the given entity type.");
}
