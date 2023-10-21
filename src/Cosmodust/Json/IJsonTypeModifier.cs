using System.Text.Json.Serialization.Metadata;

namespace Cosmodust.Json;

public interface IJsonTypeModifier
{
    void Modify(JsonTypeInfo jsonTypeInfo);
}
