using System.Text.Json;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Tracking;

namespace Cosmodust.Store;

/// <summary>
/// Represents a document store that provides access to a database and allows for creating document sessions.
/// </summary>
public sealed class DocumentStore : IDocumentStore
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _options;
    private readonly EntityConfigurationHolder _entityConfiguration;
    private readonly SqlParameterCache _sqlParameterCache;

    public DocumentStore(
        IDatabase database,
        JsonSerializerOptions? options = default,
        EntityConfigurationHolder? entityConfiguration = default,
        SqlParameterCache? sqlParameterCache = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        _database = database;
        _options = options ?? new JsonSerializerOptions();
        _entityConfiguration = entityConfiguration
                              ?? new EntityConfigurationHolder();
        _sqlParameterCache = sqlParameterCache
                          ?? new SqlParameterCache();
    }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(
            new ChangeTracker(_entityConfiguration),
            _database,
            _entityConfiguration,
            _sqlParameterCache);
    }

    /// <summary>
    /// Configures the entities in the document store using the provided <paramref name="builder"/> action.
    /// </summary>
    /// <param name="builder">The action used to configure the entities.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore BuildModel(Action<ModelBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var modelBuilder = new ModelBuilder(_options);
        builder(modelBuilder);

        foreach (var configuration in modelBuilder.Build())
            _entityConfiguration.Add(configuration);

        // marks the entity configuration object as read-only
        _entityConfiguration.Configure();

        return this;
    }
}
