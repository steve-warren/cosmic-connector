using CosmicConnector.Query;

namespace CosmicConnector;
public sealed class DocumentStore : IDocumentStore
{
    public DocumentStore(IDatabase database)
    {
        Database = database;
        EntityConfiguration = new();
        database.EntityConfiguration = EntityConfiguration;
    }

    public IDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(this, EntityConfiguration, Database);
    }

    /// <summary>
    /// Maps the entity with the specified database name, container name, and partition key selector.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to configure.</typeparam>
    /// <param name="containerName">The name of the container.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore ConfigureEntity<TEntity>(string containerName, Func<TEntity, string> idSelector, Func<TEntity, string>? partitionKeySelector = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        var idSelectorInstance = new StringSelector<TEntity>(idSelector);

        var partitionKeySelectorInstance = partitionKeySelector is null ? NullStringSelector.Instance : new StringSelector<TEntity>(partitionKeySelector);

        var entityConfiguration = new EntityConfiguration(typeof(TEntity), containerName, idSelectorInstance, partitionKeySelectorInstance);

        EntityConfiguration.Add(entityConfiguration);

        return this;
    }

    internal void EnsureConfigured<TEntity>()
    {
        _ = EntityConfiguration.Get(typeof(TEntity)) ??
            throw new InvalidOperationException($"No configuration has been registered for type {typeof(TEntity).FullName}.");
    }
}
