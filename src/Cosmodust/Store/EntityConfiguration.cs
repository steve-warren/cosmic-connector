using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Shared;
using Cosmodust.Tracking;

namespace Cosmodust.Store;

public record EntityConfiguration(Type EntityType)
{
    public string ContainerName { get; init; } = string.Empty;
    public IStringSelector IdGetter { get; init; } = NullStringSelector.Instance;
    public IStringSetter IdSetter { get; init; } = NullStringSetter.Instance;
    public IStringSelector PartitionKeySelector { get; init; } = NullStringSelector.Instance;
    internal DomainEventAccessor DomainEventAccessor { get; init; } = DomainEventAccessor.Null;
    public string PartitionKeyName { get; init; } = "";
    public string IdPropertyName { get; init; } = "";
    internal bool IsIdPropertyDefinedInEntity { get; init; }
    public IReadOnlyCollection<FieldAccessor> Fields { get; init; } = Array.Empty<FieldAccessor>();
    public IReadOnlyCollection<PropertyAccessor> Properties { get; init; } = Array.Empty<PropertyAccessor>();
    public IReadOnlyCollection<ShadowProperty> JsonProperties { get; init; } = Array.Empty<ShadowProperty>();
    public bool IsPartitionKeyDefinedInEntity { get; init; }

    public EntityEntry CreateEntry(
        ShadowPropertyProvider provider,
        object entity,
        EntityState state)
    {
        Ensure.NotNull(entity);
        Ensure.Equals(entity.GetType(), EntityType, "Entity types must match.");

        var id = IdGetter.GetString(entity);
        var partitionKey = PartitionKeySelector.GetString(entity);

        Ensure.NotNullOrWhiteSpace(
            argument: partitionKey,
            message: "Partition key is empty.");

        Ensure.NotNullOrWhiteSpace(
            argument: id,
            message: "Id is empty.");

        var entry = new EntityEntry
        {
            Id = id,
            ContainerName = ContainerName,
            PartitionKey = partitionKey,
            PartitionKeyName = PartitionKeyName,
            Entity = entity,
            EntityType = EntityType,
            Provider = provider,
            State = state,
            DomainEventAccessor = DomainEventAccessor
        };

        entry.ReadShadowProperties();

        if (state == EntityState.Added)
        {
            foreach (var jsonProperty in JsonProperties)
                entry.WriteShadowProperty(jsonProperty.PropertyName, value: jsonProperty.DefaultValue);

            return entry;
        }

        return entry;
    }
}
