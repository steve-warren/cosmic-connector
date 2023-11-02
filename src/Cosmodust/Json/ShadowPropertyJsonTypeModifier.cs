using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

/// <summary>
/// A JSON type modifier that adds shadow properties to JSON objects based on entity configuration.
/// </summary>
public class ShadowPropertyJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationProvider _entityConfigurationProvider;

    public ShadowPropertyJsonTypeModifier(
        EntityConfigurationProvider entityConfigurationProvider)
    {
        _entityConfigurationProvider = entityConfigurationProvider;
    }

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!_entityConfigurationProvider.TryGetEntityConfiguration(jsonTypeInfo.Type, out var entityConfiguration))
            return;

        foreach (var shadowProperty in entityConfiguration.JsonProperties)
        {
            var shadowJsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(
                shadowProperty.PropertyType,
                shadowProperty.PropertyName);

            shadowJsonPropertyInfo.Set = shadowProperty.WriteProperty;
            shadowJsonPropertyInfo.Get = shadowProperty.ReadProperty;

            jsonTypeInfo.Properties.Add(shadowJsonPropertyInfo);
        }
    }
}
