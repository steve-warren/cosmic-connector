using System.Diagnostics;
using System.IO.Pipelines;
using Cosmodust.Serialization;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos;

public class QueryFacade
{
    private readonly Database _database;
    private readonly SqlParameterObjectTypeCache _sqlParameterObjectTypeCache;

    public QueryFacade(
        CosmosClient client,
        string databaseName,
        SqlParameterObjectTypeCache sqlParameterObjectTypeCache)
    {
        _database = client.GetDatabase(databaseName);
        _sqlParameterObjectTypeCache = sqlParameterObjectTypeCache;
    }

    public async ValueTask ExecuteQueryAsync(
        PipeWriter writer,
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

        var flushTask = ValueTask.FromResult(new FlushResult());

        try
        {
            while (feed.HasMoreResults)
            {
                var readNextTask = feed.ReadNextAsync();

                await flushTask.ConfigureAwait(false);
                using var response = await readNextTask.ConfigureAwait(false);

                Debug.WriteLine($"{response.Headers.RequestCharge} RUs");

                // ReSharper disable once UseAwaitUsing
                // underlying stream is MemoryStream
                using var stream = response.Content;

                CopyStreamToWriter(
                    writer: writer,
                    stream: stream);

                flushTask = writer.FlushAsync();
            }

            await writer.CompleteAsync().ConfigureAwait(false);
        }

        finally
        {
            if (!flushTask.IsCompleted)
                await flushTask.ConfigureAwait(false);
        }
    }

    private static void CopyStreamToWriter(PipeWriter writer, Stream stream)
    {
        var span = writer.GetSpan((int) stream.Length);

        Debug.Assert(stream is MemoryStream, message: "stream is not a MemoryStream.");

        var bytesRead = stream.Read(span);

        writer.Advance(bytesRead);
    }
}
