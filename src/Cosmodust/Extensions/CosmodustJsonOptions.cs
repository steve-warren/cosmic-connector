using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Json;

namespace Cosmodust.Extensions;

public class CosmodustJsonOptions
{
    private readonly HashSet<IJsonTypeModifier> _jsonTypeModifiers = new();
    private JsonNamingPolicy _jsonNamingPolicy = JsonNamingPolicy.CamelCase;

    public bool SerializeEnumsToStrings { get; set; }

    public CosmodustJsonOptions SerializePrivateProperties()
    {
        return this;
    }

    public CosmodustJsonOptions SerializePrivateFields()
    {
        return this;
    }

    public CosmodustJsonOptions CamelCase()
    {
        return this;
    }

    public CosmodustJsonOptions SerializeEntityTypeInfo()
    {
        return this;
    }

    public CosmodustJsonOptions WithJsonTypeModifier(IJsonTypeModifier jsonTypeModifier)
    {
        _jsonTypeModifiers.Add(jsonTypeModifier);

        return this;
    }

    public JsonSerializerOptions Build()
    {
        var jsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();

        foreach (var action in _jsonTypeModifiers)
            jsonTypeInfoResolver.Modifiers.Add(action.Modify);

        var options =  new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = _jsonNamingPolicy,
            TypeInfoResolver = jsonTypeInfoResolver
        };

        if (SerializeEnumsToStrings)
            options.Converters.Add(new JsonStringEnumConverter(_jsonNamingPolicy));

        return options;
    }
}
