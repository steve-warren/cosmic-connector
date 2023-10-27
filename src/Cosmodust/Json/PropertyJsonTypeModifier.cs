using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

public sealed class PropertyJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationHolder _entityConfigurationHolder;

    public PropertyJsonTypeModifier(EntityConfigurationHolder entityConfigurationHolder)
    {
        _entityConfigurationHolder = entityConfigurationHolder;
    }

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!_entityConfigurationHolder.TryGet(jsonTypeInfo.Type, out var entityConfiguration))
            return;

        foreach (var property in entityConfiguration.Properties)
        {
            var fieldName = JsonNamingPolicy.CamelCase.ConvertName(property.PropertyName);
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(property.PropertyType, fieldName);
            jsonPropertyInfo.Get = property.Getter;
            jsonPropertyInfo.Set = property.Setter;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }
}
