using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Json;

namespace Cosmodust.Extensions;

public class CosmodustJsonOptions
{
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

    internal JsonSerializerOptions Build(
        IEnumerable<IJsonTypeModifier> jsonTypeModifiers)
    {
        var jsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();

        foreach (var action in jsonTypeModifiers)
            jsonTypeInfoResolver.Modifiers.Add(action.Modify);

        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = jsonTypeInfoResolver
        };
    }
}
