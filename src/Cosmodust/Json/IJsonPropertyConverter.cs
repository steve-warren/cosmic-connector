using System.Text.Json;

namespace Cosmodust.Json;

public interface IJsonPropertyConverter
{
    bool Convert(
        string propertyName,
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer);
}
