using System.Text.Json;

namespace Cosmodust.Json;

internal sealed class CosmosMetadataJsonPropertyConverter : IJsonPropertyConverter
{
    private static readonly Dictionary<string, string?> s_cosmosDbMetadataFieldNames = new()
    {
        { "_rid", null },
        { "_self", null },
        { "_attachments", null },
        { "_ts", null }
    };

    public bool Convert(
        string propertyName,
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer)
    {
        if (s_cosmosDbMetadataFieldNames.ContainsKey(propertyName))
        {
            reader.Skip();
            return true;
        }

        return false;
    }
}
