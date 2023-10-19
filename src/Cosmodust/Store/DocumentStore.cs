using Cosmodust.Session;
using Cosmodust.Tracking;

namespace Cosmodust.Store;

public sealed class DocumentStore : IDocumentStore
{
    public DocumentStore(IDatabase database, EntityConfigurationHolder? entityConfiguration = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        Database = database;
        EntityConfiguration = entityConfiguration
                              ?? new EntityConfigurationHolder();
    }

    public IDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(this, new ChangeTracker(EntityConfiguration), Database);
    }

    /// <summary>
    /// Configures the entities in the document store using the provided <paramref name="builder"/> action.
    /// </summary>
    /// <param name="builder">The action used to configure the entities.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore BuildModel(Action<ModelBuilder> builder)
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

    internal EntityConfiguration GetConfiguration<TEntity>() =>
        EntityConfiguration.Get(typeof(TEntity)) ??
            throw new InvalidOperationException($"No configuration has been registered for type {typeof(TEntity).FullName}.");
}
