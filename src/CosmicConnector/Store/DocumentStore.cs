namespace CosmicConnector;
public sealed class DocumentStore : IDocumentStore
{

    public DocumentStore(IDatabaseFacade databaseFacade)
    {
        DatabaseFacade = databaseFacade;
        EntityConfiguration = new();
        databaseFacade.EntityConfiguration = EntityConfiguration;
    }

    internal IdentityAccessor IdentityAccessor { get; } = new();
    public IDatabaseFacade DatabaseFacade { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(this, IdentityAccessor, DatabaseFacade);
    }

    /// <summary>
    /// Maps the entity with the specified database name, container name, and partition key selector.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to configure.</typeparam>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="containerName">The name of the container.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore ConfigureEntity<TEntity>(string databaseName, string containerName) where TEntity : class
    {
        ArgumentException.ThrowIfNullOrEmpty(databaseName);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        var entityConfiguration = new EntityConfiguration(typeof(TEntity), databaseName, containerName);

        EntityConfiguration.Add(entityConfiguration);

        IdentityAccessor.RegisterType<TEntity>();

        return this;
    }

    internal void EnsureConfigured<TEntity>() where TEntity : class
    {
        _ = EntityConfiguration.Get(typeof(TEntity)) ??
            throw new InvalidOperationException($"No configuration has been registered for type {typeof(TEntity).FullName}.");
    }
}
