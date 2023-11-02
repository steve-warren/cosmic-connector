using System.Text.Json;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Shared;
using Cosmodust.Tracking;

namespace Cosmodust.Store;

/// <summary>
/// Represents a document store that provides access to a database and allows for creating document sessions.
/// </summary>
public class DocumentStore : IDocumentStore
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _options;
    private readonly EntityConfigurationProvider _entityConfiguration;
    private readonly SqlParameterObjectTypeCache _sqlParameterObjectTypeCache;
    private readonly JsonPropertyStore _jsonPropertyStore;

    public DocumentStore(
        IDatabase database,
        JsonSerializerOptions? options = default,
        EntityConfigurationProvider? entityConfiguration = default,
        SqlParameterObjectTypeCache? sqlParameterCache = default,
        JsonPropertyStore? shadowPropertyStore = default)
    {
        Ensure.NotNull(database);

        _database = database;
        _options = options ?? new JsonSerializerOptions();
        _entityConfiguration = entityConfiguration
                               ?? new EntityConfigurationProvider();
        _sqlParameterObjectTypeCache = sqlParameterCache
                             ?? new SqlParameterObjectTypeCache();
        _jsonPropertyStore = shadowPropertyStore
                               ?? new JsonPropertyStore();
    }

    public DocumentSession CreateSession()
    {
        return new DocumentSession(
            _database,
            _entityConfiguration,
            _sqlParameterObjectTypeCache,
            _jsonPropertyStore);
    }

    /// <summary>
    /// Configures the entities in the document store using the provided <paramref name="builder"/> action.
    /// </summary>
    /// <param name="builder">The action used to configure the entities.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore DefineModel(Action<ModelBuilder> builder)
    {
        Ensure.NotNull(builder);

        var modelBuilder = new ModelBuilder(_options, _jsonPropertyStore);
        builder(modelBuilder);

        foreach (var configuration in modelBuilder.Build())
            _entityConfiguration.AddEntityConfiguration(configuration);

        // marks the entity configuration object as read-only
        _entityConfiguration.Build();

        return this;
    }
}
