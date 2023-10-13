using CosmoDust.Query;

namespace CosmoDust;
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

        return this;
    }

    internal void EnsureConfigured<TEntity>()
    {
        _ = EntityConfiguration.Get(typeof(TEntity)) ??
            throw new InvalidOperationException($"No configuration has been registered for type {typeof(TEntity).FullName}.");
    }
}
