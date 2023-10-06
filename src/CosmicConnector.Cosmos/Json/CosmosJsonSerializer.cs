using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;

namespace CosmicConnector.Cosmos;

internal sealed class CosmosJsonSerializer : CosmosSerializer
{
    public CosmosJsonSerializer(JsonSerializerOptions options)
    {
        JsonObjectSerializer = new JsonObjectSerializer(options);
    }

    public JsonObjectSerializer JsonObjectSerializer { get; }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream.CanSeek && stream.Length == 0)
                return default!;

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
                return (T) (object) stream;

            return (T) JsonObjectSerializer.Deserialize(stream, typeof(T), default)!;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();

        try
        {
            JsonObjectSerializer.Serialize(stream, input, typeof(T), default);

            stream.Position = 0;

            return stream;
        }

        catch
        {
            stream.Dispose();

            throw;
        }
    }
}
