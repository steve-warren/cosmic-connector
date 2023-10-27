using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

public sealed class PartitionKeyJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationHolder _entityConfigurations;

    public PartitionKeyJsonTypeModifier(EntityConfigurationHolder entityConfigurations)
    {
        _entityConfigurations = entityConfigurations;
    }
    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!_entityConfigurations.TryGet(jsonTypeInfo.Type, out var entityConfiguration))
            return;

        var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(
            propertyType: typeof(string),
            name: entityConfiguration!.PartitionKeyDocumentPropertyName);

        jsonPropertyInfo.Get = entityConfiguration.PartitionKeySelector.GetString;

        jsonTypeInfo.Properties.Add(jsonPropertyInfo);
    }
}
