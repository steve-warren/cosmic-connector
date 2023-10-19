using System.Text.Json.Serialization.Metadata;

namespace Cosmodust.Cosmos.Json;

public interface IJsonTypeModifier
{
    void Modify(JsonTypeInfo jsonTypeInfo);
}
