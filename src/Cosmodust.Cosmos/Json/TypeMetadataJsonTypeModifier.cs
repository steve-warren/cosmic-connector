using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Cosmos.Json;

public sealed class TypeMetadataJsonTypeModifier : IJsonTypeModifier
{
    private static readonly Func<object, object?> s_typeFunc = obj => obj.GetType().Name;

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(typeof(string), "__type");
        jsonPropertyInfo.Get = s_typeFunc;

        jsonTypeInfo.Properties.Add(jsonPropertyInfo);
    }
}
