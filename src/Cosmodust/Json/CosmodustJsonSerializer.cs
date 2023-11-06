using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Memory;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Json;

public sealed class CosmodustJsonSerializer : CosmosSerializer
{
    private readonly IMemoryStreamProvider _memoryStreamProvider;

    public CosmodustJsonSerializer(
        JsonSerializerOptions options,
        IMemoryStreamProvider? memoryStreamProvider = default)
    {
        Options = options;
        _memoryStreamProvider = memoryStreamProvider ?? DefaultMemoryStreamProvider.Instance;
    }
    
    public JsonSerializerOptions Options { get; }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
            return stream.Length == 0 ? default! : JsonSerializer.Deserialize<T>(stream, Options)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = _memoryStreamProvider.GetMemoryStream(tag: "CosmodustJsonSerializer")!;

        try
        {
            JsonSerializer.Serialize(stream, input, typeof(T), Options);

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
