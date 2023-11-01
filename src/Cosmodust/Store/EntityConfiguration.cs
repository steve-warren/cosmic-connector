using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Shared;
using Cosmodust.Tracking;

namespace Cosmodust.Store;

public record EntityConfiguration(Type EntityType)
{
    public string ContainerName { get; init; } = string.Empty;
    public IStringSelector IdSelector { get; init; } = NullStringSelector.Instance;
    public IStringSelector PartitionKeySelector { get; init; } = NullStringSelector.Instance;
    public string PartitionKeyName { get; init; } = "";
    public IReadOnlyCollection<FieldAccessor> Fields { get; init; } = Array.Empty<FieldAccessor>();
    public IReadOnlyCollection<PropertyAccessor> Properties { get; init; } = Array.Empty<PropertyAccessor>();
    public IReadOnlyCollection<ShadowProperty> ShadowProperties { get; init; } = Array.Empty<ShadowProperty>();

    public EntityEntry CreateEntry(
        JsonSerializerPropertyStore store,
        object entity,
        EntityState state)
    {
        Ensure.NotNull(entity);
        Ensure.Equals(entity.GetType(), EntityType, "Entity types must match.");

        var id = IdSelector.GetString(entity);
        var partitionKey = PartitionKeySelector.GetString(entity);

        Ensure.NotNullOrWhiteSpace(
            argument: partitionKey,
            message: "Partition key is empty.");

        var entry = new EntityEntry
        {
            Id = id,
            ContainerName = ContainerName,
            PartitionKey = partitionKey,
            Entity = entity,
            EntityType = EntityType,
            Store = store,
            State = state
        };

        entry.FetchJsonPropertiesFromSerializer();

        if (state != EntityState.Added)
            return entry;

        foreach (var shadowProperty in ShadowProperties)
            entry.WriteShadowProperty(shadowProperty.PropertyName, value: shadowProperty.DefaultValue);

        return entry;
    }
}
