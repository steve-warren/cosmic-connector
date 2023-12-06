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
    private readonly EntityConfigurationProvider _entityConfiguration;
    private readonly SqlParameterObjectTypeResolver _sqlParameterObjectTypeResolver;
    private readonly JsonPropertyBroker _jsonPropertyBroker;

    public DocumentStore(
        IDatabase database,
        EntityConfigurationProvider entityConfiguration,
        SqlParameterObjectTypeResolver sqlParameterCache,
        JsonPropertyBroker shadowPropertyStore)
    {
        Ensure.NotNull(database);

        _database = database;
        _entityConfiguration = entityConfiguration;
        _sqlParameterObjectTypeResolver = sqlParameterCache;
        _jsonPropertyBroker = shadowPropertyStore;
    }

    public DocumentSession CreateSession()
    {
        return new DocumentSession(
            _database,
            _entityConfiguration,
            _sqlParameterObjectTypeResolver,
            _jsonPropertyBroker);
    }
}
