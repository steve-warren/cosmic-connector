using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

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

        if (!_entityConfigurationHolder.TryGet(jsonTypeInfo.Type, out var entityConfiguration))
            return;

        foreach (var field in entityConfiguration.Fields)
        {
            var fieldName = JsonNamingPolicy.CamelCase.ConvertName(field.FieldName);
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, fieldName);
            jsonPropertyInfo.Get = field.Getter;
            jsonPropertyInfo.Set = field.Setter;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }
}
