using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CosmoDust.Cosmos.Memory;
using Microsoft.Azure.Cosmos;

namespace CosmoDust.Cosmos;

public sealed class CosmosJsonSerializer : CosmosSerializer
{
    private readonly IMemoryStreamProvider _memoryStreamProvider;
    private readonly JsonSerializerOptions _options;
    private readonly List<IJsonTypeModifier> _jsonTypeModifiers = new();

    public CosmosJsonSerializer(IEnumerable<IJsonTypeModifier> jsonTypeModifiers, IMemoryStreamProvider? memoryStreamProvider = default)
    {
        _memoryStreamProvider = memoryStreamProvider ?? DefaultMemoryStreamProvider.Instance;
        _jsonTypeModifiers.AddRange(jsonTypeModifiers);

        var jsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();

        foreach (var action in _jsonTypeModifiers.Select(m => (Action<JsonTypeInfo>) m.Modify))
            jsonTypeInfoResolver.Modifiers.Add(action);

        _options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = jsonTypeInfoResolver
        };
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream.Length == 0)
                return default!;

            return JsonSerializer.Deserialize<T>(stream, _options)!;
        }
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
