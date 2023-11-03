using System.Text.Json;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Shared;
using Cosmodust.Tracking;

namespace Cosmodust.Store;

/// <summary>
/// Represents a document broker that provides access to a database and allows for creating document sessions.
/// </summary>
public class DocumentStore : IDocumentStore
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _options;
    private readonly EntityConfigurationProvider _entityConfiguration;
    private readonly SqlParameterObjectTypeResolver _sqlParameterObjectTypeResolver;
    private readonly JsonPropertyBroker _jsonPropertyBroker;

    public DocumentStore(
        IDatabase database,
        JsonSerializerOptions? options = default,
        EntityConfigurationProvider? entityConfiguration = default,
        SqlParameterObjectTypeResolver? sqlParameterCache = default,
        JsonPropertyBroker? shadowPropertyStore = default)
    {
        Ensure.NotNull(database);

        _database = database;
        _options = options ?? new JsonSerializerOptions();
        _entityConfiguration = entityConfiguration
                               ?? new EntityConfigurationProvider();
        _sqlParameterObjectTypeResolver = sqlParameterCache
                             ?? new SqlParameterObjectTypeResolver();
        _jsonPropertyBroker = shadowPropertyStore
                               ?? new JsonPropertyBroker();
    }

    public DocumentSession CreateSession()
    {
        return new DocumentSession(
            _database,
            _entityConfiguration,
            _sqlParameterObjectTypeResolver,
            _jsonPropertyBroker);
    }

    /// <summary>
    /// Configures the entities in the document broker using the provided <paramref name="builder"/> action.
    /// </summary>
    /// <param name="builder">The action used to configure the entities.</param>
    /// <returns>The current instance of the <see cref="DocumentStore"/> class.</returns>
    public DocumentStore DefineModel(Action<ModelBuilder> builder)
    {
        Ensure.NotNull(builder);

        var modelBuilder = new ModelBuilder(_options, _jsonPropertyBroker);
        builder(modelBuilder);

        foreach (var configuration in modelBuilder.Build())
            _entityConfiguration.AddEntityConfiguration(configuration);

        // marks the entity configuration object as read-only
        _entityConfiguration.Build();

        return this;
    }
}
