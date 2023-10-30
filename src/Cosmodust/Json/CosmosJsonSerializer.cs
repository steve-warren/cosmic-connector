using System.Text.Json;
using Cosmodust.Memory;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Json;

public sealed class CosmosJsonSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly IMemoryStreamProvider _memoryStreamProvider;

    public CosmosJsonSerializer(JsonSerializerOptions options, IMemoryStreamProvider? memoryStreamProvider = default)
    {
        _options = options;
        _memoryStreamProvider = memoryStreamProvider ?? DefaultMemoryStreamProvider.Instance;
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
            return stream.Length == 0 ? default! : JsonSerializer.Deserialize<T>(stream, _options)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = _memoryStreamProvider.GetMemoryStream(tag: "CosmosJsonSerializer")!;

        try
        {
            JsonSerializer.Serialize(stream, input, typeof(T), _options);

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
