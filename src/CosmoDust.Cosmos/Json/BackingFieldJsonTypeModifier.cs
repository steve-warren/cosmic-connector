using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace CosmoDust.Cosmos;

public class BackingFieldJsonTypeModifier : IJsonTypeModifier
{
    public void Serialize(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        // return if we are not serializing private fields

        // todo: do we need to cache this?
        foreach (var field in jsonTypeInfo.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
            jsonPropertyInfo.Get = field.GetValue;
            jsonPropertyInfo.Set = field.SetValue;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }
}
