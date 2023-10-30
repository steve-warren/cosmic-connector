using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

public sealed class PropertyJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationProvider _entityConfigurationProvider;

    public PropertyJsonTypeModifier(EntityConfigurationProvider entityConfigurationProvider)
    {
        _entityConfigurationProvider = entityConfigurationProvider;
    }

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!_entityConfigurationProvider.TryGetEntityConfiguration(jsonTypeInfo.Type, out var entityConfiguration))
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
