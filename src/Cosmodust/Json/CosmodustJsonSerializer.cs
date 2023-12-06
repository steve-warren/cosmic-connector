using System.Text.Json;
using Cosmodust.Extensions;
using Cosmodust.Memory;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Cosmodust.Json;

public sealed class CosmodustJsonSerializer : CosmosSerializer
{
    private readonly IMemoryStreamProvider _memoryStreamProvider;

    public CosmodustJsonSerializer(
        CosmodustJsonOptions options,
        IMemoryStreamProvider? memoryStreamProvider = default)
    {
        Options = options.Build();
        _memoryStreamProvider = memoryStreamProvider ?? DefaultMemoryStreamProvider.Instance;
    }

    public JsonSerializerOptions Options { get; }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
            return stream.Length switch
            {
                0 => default,
                _ => JsonSerializer.Deserialize<T>(stream, Options)
            } ?? throw new JsonSerializationException(message: "Object cannot be null.");
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
