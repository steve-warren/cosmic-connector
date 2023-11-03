using System.Text.Json;

namespace Cosmodust.Json;

internal sealed class DocumentCollectionPropertyConverter : IJsonPropertyConverter
{
    private readonly Dictionary<string, string> _propertyNames;

    public DocumentCollectionPropertyConverter(Dictionary<string, string> propertyNames)
    {
        _propertyNames = propertyNames;
    }

    public bool Convert(
        string propertyName,
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer)
    {
        if (!_propertyNames.TryGetValue(propertyName, out var newPropertyName))
            return false;

        writer.WritePropertyName(newPropertyName);
        return true;
    }
}
