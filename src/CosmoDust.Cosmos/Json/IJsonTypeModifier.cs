using System.Text.Json.Serialization.Metadata;

namespace Cosmodust.Cosmos;

public interface IJsonTypeModifier
{
    void Modify(JsonTypeInfo jsonTypeInfo);
}
