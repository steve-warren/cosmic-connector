using System.Text.Json.Serialization.Metadata;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Store;

namespace Cosmodust.Json;

/// <summary>
/// A JSON type modifier that adds shadow properties to JSON objects based on entity configuration.
/// </summary>
public class ShadowPropertyJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationHolder _entityConfigurationHolder;

    public ShadowPropertyJsonTypeModifier(
        EntityConfigurationHolder entityConfigurationHolder)
    {
        _entityConfigurationHolder = entityConfigurationHolder;
    }

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!_entityConfigurationHolder.TryGet(jsonTypeInfo.Type, out var entityConfiguration))
            return;

        foreach (var shadowProperty in entityConfiguration.ShadowProperties)
        {
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(
                shadowProperty.PropertyType,
                shadowProperty.PropertyName);

            jsonPropertyInfo.Set = shadowProperty.SetValue;
            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }
}
