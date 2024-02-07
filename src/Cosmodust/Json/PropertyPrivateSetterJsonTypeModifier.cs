using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Shared;
using Cosmodust.Store;

namespace Cosmodust.Json;

public class PropertyPrivateSetterJsonTypeModifier : IJsonTypeModifier
{
    private readonly EntityConfigurationProvider _entityConfigurationProvider;

    public PropertyPrivateSetterJsonTypeModifier(EntityConfigurationProvider entityConfigurationProvider)
    {
        _entityConfigurationProvider = entityConfigurationProvider;
    }

    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object
            || !_entityConfigurationProvider.HasEntityConfiguration(jsonTypeInfo.Type))
            return;

        foreach (var jsonPropertyInfo in jsonTypeInfo.Properties)
        {
            if (jsonPropertyInfo.Set is not null)
                continue;

            var propertyInfo = jsonTypeInfo.Type.GetProperty(
                jsonPropertyInfo.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            var setMethod = propertyInfo?.GetSetMethod(nonPublic: true);

            if (setMethod is null || setMethod.IsPublic)
                continue;
            
            Ensure.NotNull(propertyInfo);

            Debug.WriteLine($"Found property {propertyInfo.Name} on type {jsonTypeInfo.Type.Name} having a private setter.");

            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            var instanceCast = Expression.Convert(instance, jsonTypeInfo.Type);
            var valueCast = Expression.Convert(value, jsonPropertyInfo.PropertyType);

            var setter = Expression.Lambda<Action<object, object?>>(
                Expression.Assign(Expression.Property(instanceCast, jsonPropertyInfo.Name), valueCast),
                instance, value).Compile();

            jsonPropertyInfo.Set = setter;
        }
    }
}
