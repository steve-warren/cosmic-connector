using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using CommunityToolkit.HighPerformance.Buffers;
using Cosmodust.Extensions;
using Cosmodust.Json;
using Cosmodust.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmodust;

public class QueryFacade
{
    private static readonly JsonReaderOptions s_readerOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip
    };

    private readonly JsonWriterOptions _jsonWriterOptions;
    private readonly Database _database;
    private readonly SqlParameterObjectTypeResolver _sqlParameterObjectTypeResolver;
    private readonly ILogger<QueryFacade> _logger;
    private readonly CosmodustQueryOptions _options;
    private readonly IReadOnlyList<IJsonPropertyConverter> _converters;

    public QueryFacade(
        CosmosClient client,
        string databaseName,
        SqlParameterObjectTypeResolver sqlParameterObjectTypeResolver,
        ILogger<QueryFacade> logger,
        CosmodustQueryOptions? options = null)
    {
        _database = client.GetDatabase(databaseName);
        _sqlParameterObjectTypeResolver = sqlParameterObjectTypeResolver;
        _logger = logger;
        _options = options ?? new CosmodustQueryOptions();
        _converters = _options.BuildConverters();
        _jsonWriterOptions = new JsonWriterOptions
        {
            Indented = _options.IndentJsonOutput,
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

                var inputBuffer = new ReadOnlySequence<byte>(stream.GetBuffer(), 0, (int) stream.Length);

                TransformJson(_converters, inputBuffer, writer);

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
        IReadOnlyList<IJsonPropertyConverter> propertyConverters,
        ReadOnlySequence<byte> inputBuffer,
        Utf8JsonWriter writer)
    {
        Debug.Assert(propertyConverters is not null);

        var reader = new Utf8JsonReader(inputBuffer, s_readerOptions);

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var propertyName = StringPool.Shared.GetOrAdd(reader.ValueSpan, Encoding.UTF8);

                    var handled = false;

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < propertyConverters.Count; i++)
                    {
                        var converter = propertyConverters[i];
                        handled = converter.Convert(propertyName, ref reader, writer);

                        if (handled)
                            break;
                    }

                    if (!handled)
                        writer.WritePropertyName(propertyName);

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
