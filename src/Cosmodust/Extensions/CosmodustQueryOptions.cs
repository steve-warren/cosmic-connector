using Cosmodust.Json;

namespace Cosmodust.Extensions;

public class CosmodustQueryOptions
{
    private readonly List<IJsonPropertyConverter> _propertyConverters = new();

    internal bool IndentJsonOutput { get; private set; }
    
    public CosmodustQueryOptions ExcludeCosmosMetadata()
    {
        _propertyConverters.Add(new CosmosMetadataJsonPropertyConverter());
        return this;
    }

    public CosmodustQueryOptions IncludeETag()
    {
        _propertyConverters.Add(new CosmosETagJsonPropertyConverter());
        return this;
    }

    public CosmodustQueryOptions WithDocumentCollectionName(
        string? documentCollectionPropertyName = null,
        string? documentCountPropertyName = null)
    {
        var properties = new Dictionary<string, string>()
        {
            { "Documents", documentCollectionPropertyName ?? "items" },
            { "_count", documentCountPropertyName ?? "itemCount" }
        };

        _propertyConverters.Add(new DocumentCollectionPropertyConverter(properties));
        return this;
    }

    public CosmodustQueryOptions FormatJson()
    {
        IndentJsonOutput = true;
        return this;
    }

    internal IReadOnlyList<IJsonPropertyConverter> BuildConverters()
    {
        return _propertyConverters.AsReadOnly();
    }
}
