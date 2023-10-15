using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Cosmodust.Cosmos;

public sealed class BackingFieldJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationHolder _entityConfigurationHolder;

    public BackingFieldJsonTypeModifier(EntityConfigurationHolder entityConfigurationHolder)
    {
        _entityConfigurationHolder = entityConfigurationHolder;
    }

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        var entityConfiguration = _entityConfigurationHolder.Get(jsonTypeInfo.Type);

        if (entityConfiguration is null)
            return;

        foreach (var field in entityConfiguration.Fields)
        {
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, field.FieldName);
            jsonPropertyInfo.Get = field.Getter;
            jsonPropertyInfo.Set = field.Setter;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }

        foreach (var property in entityConfiguration.Properties)
        {
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(property.PropertyType, property.PropertyName);
            jsonPropertyInfo.Get = property.Getter;
            jsonPropertyInfo.Set = property.Setter;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }
}
