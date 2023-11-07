using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

public sealed class TypeMetadataJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationProvider _entityConfigurationProvider;
    private static readonly Func<object, object?> s_typeFunc = obj => obj.GetType().Name;

    public TypeMetadataJsonTypeModifier(EntityConfigurationProvider entityConfigurationProvider)
    {
        _entityConfigurationProvider = entityConfigurationProvider;
    }
    
    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!_entityConfigurationProvider.HasEntityConfiguration(jsonTypeInfo.Type))
            return;

        var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(typeof(string), "_type");
        jsonPropertyInfo.Get = s_typeFunc;

        jsonTypeInfo.Properties.Add(jsonPropertyInfo);
    }
}
