namespace CosmicConnector;
public sealed class DocumentStore : IDocumentStore
{
    private readonly EntityMappingCollection _entityMaps = new();

    public DocumentStore(IDatabaseFacade databaseFacade, EntityMappingCollection entityMaps)
    {
        DatabaseFacade = databaseFacade;
        _entityMaps = entityMaps;
    }

    internal IdentityAccessor IdAccessor { get; } = new();
    public IDatabaseFacade DatabaseFacade { get; }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(IdAccessor, DatabaseFacade);
    }

    /// <summary>
    /// Maps the entity with the specified database name, container name, and partition key selector.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to configure.</typeparam>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="containerName">The name of the container.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore MapEntity<TEntity>(string databaseName, string containerName) where TEntity : class
    {
        ArgumentException.ThrowIfNullOrEmpty(databaseName);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        var entityConfiguration = new EntityMapping(typeof(TEntity), databaseName, containerName);

        _entityMaps.Add(entityConfiguration);

        IdAccessor.RegisterType<TEntity>();

        return this;
    }
}
