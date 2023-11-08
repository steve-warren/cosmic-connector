using Cosmodust.Json;

namespace Cosmodust.Extensions;

public class CosmodustQueryOptions
{
    public bool IndentJsonOutput { get; set; } = false;
    public bool ExcludeCosmosMetadata { get; set; } = true;
    public bool IncludeETag { get; set; } = true;
    public bool RenameDocumentCollectionProperties { get; set; } = true;
    public string DocumentCollectionPropertyName { get; set; } = "items";
    public string DocumentCollectionCountPropertyName { get; set; } = "itemCount";
}
