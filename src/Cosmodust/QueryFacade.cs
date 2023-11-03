using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text.Json;
using Cosmodust.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmodust.Cosmos;

public class QueryFacade
{
    private const string ETagPropertyName = "_etag";

    private readonly Database _database;
    private readonly SqlParameterObjectTypeCache _sqlParameterObjectTypeCache;
    private readonly ILogger<QueryFacade> _logger;

    private static readonly Dictionary<string, string?> s_cosmosDbMetadataFieldNames = new()
    {
        { "_rid", null },
        { "_self", null },
        { "_attachments", null },
        { "_ts", null }
    };

    private static readonly Dictionary<string, string> s_dictionary = new()
    {
        { "Documents", "items" },
        { "_count", "itemCount" }
    };

    public QueryFacade(
        CosmosClient client,
        string databaseName,
        SqlParameterObjectTypeCache sqlParameterObjectTypeCache,
        ILogger<QueryFacade> logger)
    {
        _database = client.GetDatabase(databaseName);
        _sqlParameterObjectTypeCache = sqlParameterObjectTypeCache;
        _logger = logger;
    }

    public async ValueTask ExecuteQueryAsync(
        PipeWriter pipeWriter,
        string containerName,
        string sql,
        object? parameters = default,
        string? partitionKey = default)
    {
        var container = _database.GetContainer(containerName);

        var query = new QueryDefinition(sql);

        foreach (var parameter in _sqlParameterObjectTypeCache.ExtractParametersFromObject(parameters))
            query.WithParameter(parameter.Name, parameter.Value);

        using var feed = container.GetItemQueryStreamIterator(query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = partitionKey is null ? null : new PartitionKey(partitionKey)
            });

        var flushTask = Task.CompletedTask;

        try
        {
            await using var writer = new Utf8JsonWriter(pipeWriter, new JsonWriterOptions
            {
                Indented = true,
                SkipValidation = true
            });

            while (feed.HasMoreResults)
            {
                var readNextTask = feed.ReadNextAsync();

                await flushTask.ConfigureAwait(false);
                using var response = await readNextTask.ConfigureAwait(false);

                Debug.WriteLine($"{response.Headers.RequestCharge} RUs");

                // ReSharper disable once UseAwaitUsing
                // underlying stream is MemoryStream
                using var stream = response.Content as MemoryStream;

                Debug.Assert(stream is not null);

                var inputBuffer = new ReadOnlySequence<byte>(stream.GetBuffer(), 0, (int) stream.Length);

                TransformJson(inputBuffer, writer);

                flushTask = writer.FlushAsync();
            }
        }

        finally
        {
            if (!flushTask.IsCompleted)
                await flushTask.ConfigureAwait(false);
        }
    }

    private static void TransformJson(ReadOnlySequence<byte> inputBuffer, Utf8JsonWriter writer)
    {
        var readerOptions = new JsonReaderOptions { CommentHandling = JsonCommentHandling.Skip };
        var reader = new Utf8JsonReader(inputBuffer, readerOptions);

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var originalPropertyName = reader.GetString();

                    Debug.Assert(originalPropertyName is not null);

                    if (s_cosmosDbMetadataFieldNames.ContainsKey(originalPropertyName))
                    {
                        reader.Skip();
                        break;
                    }

                    var newPropertyName = s_dictionary.GetValueOrDefault(
                        originalPropertyName, 
                        originalPropertyName);
                    writer.WritePropertyName(newPropertyName);

                    if (originalPropertyName == ETagPropertyName)
                    {
                        reader.Read();

                        var etagValue = reader.GetString();
                        writer.WriteStringValue(etagValue);
                    }

                    continue;

                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    break;

                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    break;

                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    break;

                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    break;

                case JsonTokenType.String:
                    writer.WriteStringValue(reader.ValueSpan);
                    break;
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;
                case JsonTokenType.None:
                case JsonTokenType.Comment:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                default:
                    writer.WriteRawValue(reader.ValueSpan, skipInputValidation: true);
                    break;
            }
        }
    }
}
