using Cosmodust.Session;

namespace Cosmodust.Store;

public sealed class DocumentStore : IDocumentStore
{
    public DocumentStore(IDatabase database, EntityConfigurationHolder? entityConfiguration = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        Database = database;
        EntityConfiguration = entityConfiguration ?? new EntityConfigurationHolder();
    }

    public IDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(this, EntityConfiguration, Database);
    }

    /// <summary>
    /// Configures the entities in the document store using the provided <paramref name="builder"/> action.
    /// </summary>
    /// <param name="builder">The action used to configure the entities.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore ConfigureModel(Action<ModelBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var modelBuilder = new ModelBuilder();
        builder(modelBuilder);

        foreach (var configuration in modelBuilder.Build())
            EntityConfiguration.Add(configuration);

        // marks the entity configuration object as read-only
        EntityConfiguration.Configure();

        return this;
    }

    internal void EnsureConfigured<TEntity>()
    {
        _ = EntityConfiguration.Get(typeof(TEntity)) ??
            throw new InvalidOperationException($"No configuration has been registered for type {typeof(TEntity).FullName}.");
    }

    internal EntityConfiguration GetConfiguration(Type type)
    {
        return EntityConfiguration.Get(type) ??
            throw new InvalidOperationException($"No configuration has been registered for type {type.FullName}.");
    }
}
