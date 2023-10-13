using System.Text.Json.Serialization.Metadata;

namespace CosmoDust.Cosmos;

public interface IJsonTypeModifier
{
    void Serialize(JsonTypeInfo jsonTypeInfo);
}
