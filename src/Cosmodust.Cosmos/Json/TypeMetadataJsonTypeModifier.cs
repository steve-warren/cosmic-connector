using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Cosmos.Json;

public sealed class TypeMetadataJsonTypeModifier : IJsonTypeModifier
{
    public TypeMetadataJsonTypeModifier(EntityConfigurationHolder entityConfigurationHolder)
    {
        _entityConfigurationHolder = entityConfigurationHolder;
    }

    private static readonly Func<object, object?> s_typeFunc = obj => obj.GetType().Name;
    private readonly EntityConfigurationHolder _entityConfigurationHolder;

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        var entityConfiguration = _entityConfigurationHolder.Get(jsonTypeInfo.Type);

        if (entityConfiguration is null)
            return;

        var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(typeof(string), "__type");
        jsonPropertyInfo.Get = s_typeFunc;

        jsonTypeInfo.Properties.Add(jsonPropertyInfo);
    }
}
