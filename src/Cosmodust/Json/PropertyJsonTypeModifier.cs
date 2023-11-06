using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;
using Newtonsoft.Json.Serialization;

namespace Cosmodust.Json;

public sealed class PropertyJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationProvider _entityConfigurationProvider;
    private readonly JsonNamingPolicy _jsonNamingPolicy;

    public PropertyJsonTypeModifier(
        EntityConfigurationProvider entityConfigurationProvider,
        JsonNamingPolicy jsonNamingPolicy)
    {
        _entityConfigurationProvider = entityConfigurationProvider;
        _jsonNamingPolicy = jsonNamingPolicy;
    }

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object
            || !_entityConfigurationProvider.TryGetEntityConfiguration(
                jsonTypeInfo.Type,
                out var entityConfiguration))
            return;

        foreach (var property in entityConfiguration.Properties)
        {
            var fieldName = _jsonNamingPolicy.ConvertName(property.PropertyName);
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(property.PropertyType, fieldName);
            jsonPropertyInfo.Get = property.Getter;
            jsonPropertyInfo.Set = property.Setter;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }
}
