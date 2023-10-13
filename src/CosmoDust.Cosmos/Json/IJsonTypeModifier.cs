using System.Text.Json.Serialization.Metadata;

namespace CosmoDust.Cosmos;

public interface IJsonTypeModifier
{
    void Modify(JsonTypeInfo jsonTypeInfo);
}
