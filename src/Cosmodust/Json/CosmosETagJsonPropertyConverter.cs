using System.Diagnostics;
using System.Text.Json;

namespace Cosmodust.Json;

internal sealed class CosmosETagJsonPropertyConverter : IJsonPropertyConverter
{
    private const string ETagPropertyName = "_etag";

    public bool Convert(
        string propertyName,
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer)
    {
        if (!string.Equals(propertyName, ETagPropertyName))
            return false;

        var read = reader.Read();

        Debug.Assert(read, "failed to read etag value.");

        var etagValue = reader.GetString();
        writer.WritePropertyName(ETagPropertyName);
        writer.WriteStringValue(etagValue);

        return true;
    }
}
