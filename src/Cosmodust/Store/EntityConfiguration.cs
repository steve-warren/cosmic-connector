using Cosmodust.Serialization;

namespace Cosmodust.Store;

public record EntityConfiguration(Type EntityType)
{
    public string ContainerName { get; init; } = string.Empty;
    public IStringSelector IdSelector { get; init; } = NullStringSelector.Instance;
    public IStringSelector PartitionKeySelector { get; init; } = NullStringSelector.Instance;
    public string PartitionKeyDocumentPropertyName { get; init; } = "";
    public IReadOnlyCollection<FieldAccessor> Fields { get; init; } = Array.Empty<FieldAccessor>();
    public IReadOnlyCollection<PropertyAccessor> Properties { get; init; } = Array.Empty<PropertyAccessor>();
}
