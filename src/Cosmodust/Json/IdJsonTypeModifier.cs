using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

public sealed class IdJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationProvider _entityConfigurations;
    private readonly JsonNamingPolicy _jsonNamingPolicy;

    public IdJsonTypeModifier(
        EntityConfigurationProvider entityConfigurations,
        JsonNamingPolicy jsonNamingPolicy)
    {
        _entityConfigurations = entityConfigurations;
        _jsonNamingPolicy = jsonNamingPolicy;
    }
    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!_entityConfigurations.TryGetEntityConfiguration(jsonTypeInfo.Type, out var entityConfiguration))
            return;

        if (entityConfiguration.IsIdPropertyDefinedInEntity)
            return;

        var idName = _jsonNamingPolicy.ConvertName(
            entityConfiguration.IdPropertyName);

        var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(
            propertyType: typeof(string),
            name: "id");

        jsonPropertyInfo.Get = entityConfiguration.IdGetter.GetString;
        jsonPropertyInfo.Set = entityConfiguration.IdSetter.SetString;

        jsonTypeInfo.Properties.Add(jsonPropertyInfo);
    }
}
