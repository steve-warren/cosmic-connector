using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using CommunityToolkit.HighPerformance.Buffers;
using Cosmodust.Extensions;
using Cosmodust.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmodust;

public class QueryFacade
{
    private readonly JsonReaderOptions _readerOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip
    };

    private readonly Dictionary<string, string> _propertyRename = new();
    private readonly JsonWriterOptions _jsonWriterOptions;
    private readonly Database _database;
    private readonly SqlParameterObjectTypeResolver _sqlParameterObjectTypeResolver;
    private readonly ILogger<QueryFacade> _logger;
    private readonly Dictionary<string, JsonModifierType> _jsonModifiers = new();

    public QueryFacade(
        CosmodustOptions options,
        CosmosClient client,
        SqlParameterObjectTypeResolver sqlParameterObjectTypeResolver,
        ILogger<QueryFacade> logger)
        : this(
            client,
            options.DatabaseId,
            sqlParameterObjectTypeResolver,
            logger,
            options.QueryOptions)
    {

    }

    public QueryFacade(
        CosmosClient client,
        string databaseName,
        SqlParameterObjectTypeResolver sqlParameterObjectTypeResolver,
        ILogger<QueryFacade> logger,
        CosmodustQueryOptions? queryOptions = null)
    {
        _database = client.GetDatabase(databaseName);
        _sqlParameterObjectTypeResolver = sqlParameterObjectTypeResolver;
        _logger = logger;
        
        var options = queryOptions ?? new CosmodustQueryOptions();

        if (options.RenameDocumentCollectionProperties)
        {
            _jsonModifiers.Add("Documents", JsonModifierType.RenameProperty);
            _jsonModifiers.Add("_count", JsonModifierType.RenameProperty);
            _propertyRename.Add("Documents", options.DocumentCollectionPropertyName);
            _propertyRename.Add("_count", options.DocumentCollectionCountPropertyName);
        }

        if (options.ExcludeCosmosMetadata)
        {
            _jsonModifiers.Add("_rid", JsonModifierType.SkipProperty);
            _jsonModifiers.Add("_self", JsonModifierType.SkipProperty);
            _jsonModifiers.Add("_attachments", JsonModifierType.SkipProperty);
            _jsonModifiers.Add("_ts", JsonModifierType.SkipProperty);
        }
        
        _jsonModifiers.Add("_etag", options.IncludeETag 
            ? JsonModifierType.EscapeStringValue 
            : JsonModifierType.SkipProperty);
        
        _jsonWriterOptions = new JsonWriterOptions
        {
            Indented = options.IndentJsonOutput,
            SkipValidation = true
        };
    }

    public async ValueTask ExecuteQueryAsync(
        PipeWriter pipeWriter,
        string containerName,
        string sql,
        object? parameters = default,
        string? partitionKey = default)
    {
        var container = _database.GetContainer(containerName);
        var query = BuildSqlQueryDefinition(sql, parameters);

        using var feed = container.GetItemQueryStreamIterator(query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = partitionKey is null ? null : new PartitionKey(partitionKey)
            });

        var flushTask = Task.CompletedTask;

        try
        {
            await using var writer = new Utf8JsonWriter(pipeWriter, _jsonWriterOptions);

            while (feed.HasMoreResults)
            {
                var readNextTask = feed.ReadNextAsync();

                await flushTask.ConfigureAwait(false);

                using var response = await readNextTask.ConfigureAwait(false);
                using var stream = response.Content as MemoryStream;

                Debug.Assert(stream is not null);
                
                TransformJson(_jsonModifiers, _propertyRename, stream, _readerOptions, writer);

                flushTask = writer.FlushAsync();
            }
        }

        finally
        {
            if (!flushTask.IsCompleted)
                await flushTask.ConfigureAwait(false);
        }
    }

    private QueryDefinition BuildSqlQueryDefinition(string sql, object? parameters)
    {
        var query = new QueryDefinition(sql);

        foreach (var parameter in _sqlParameterObjectTypeResolver.ExtractParametersFromObject(parameters))
            query.WithParameter(parameter.Name, parameter.Value);

        return query;
    }

    private static void TransformJson(
        Dictionary<string, JsonModifierType> jsonModifiers,
        Dictionary<string, string> propertyRename,
        MemoryStream stream,
        JsonReaderOptions jsonReaderOptions,
        Utf8JsonWriter writer)
    {
        var reader = new Utf8JsonReader(stream.GetBuffer().AsSpan(0, (int) stream.Length), jsonReaderOptions);

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var propertyName = StringPool.Shared.GetOrAdd(reader.ValueSpan, Encoding.UTF8);

                    if (!jsonModifiers.TryGetValue(propertyName, out var jsonModifier))
                        writer.WritePropertyName(reader.ValueSpan);

                    else switch (jsonModifier)
                    {
                        case JsonModifierType.SkipProperty:
                            reader.Skip();
                            continue;
                        case JsonModifierType.EscapeStringValue:
                            {
                                var etagValue = reader.GetString(); // todo perf - avoid string alloc?
                                writer.WritePropertyName(propertyName);
                                writer.WriteStringValue(etagValue);
                                continue;
                            }
                        case JsonModifierType.RenameProperty:
                            writer.WritePropertyName(propertyRename[propertyName]);
                            continue;
                        default:
                            throw new JsonException("Unable to modify property.");
                    }
                    
                    continue;

                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    continue;

                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    continue;

                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    continue;

                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    continue;

                case JsonTokenType.String:
                    writer.WriteStringValue(reader.ValueSpan);
                    continue;
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    continue;
                case JsonTokenType.None:
                case JsonTokenType.Comment:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                default:
                    writer.WriteRawValue(reader.ValueSpan, skipInputValidation: true);
                    continue;
            }
        }
    }
}

internal enum JsonModifierType
{
    None,
    SkipProperty,
    EscapeStringValue,
    RenameProperty
}
