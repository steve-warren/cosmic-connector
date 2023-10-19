namespace Cosmodust.Store;

public class EntityConfigurationHolder
{
    private IDictionary<Type, EntityConfiguration> _mappings =
        new Dictionary<Type, EntityConfiguration>();

    public EntityConfiguration? Get(Type entityType) =>
        _mappings.TryGetValue(entityType, out var mapping)
            ? mapping
            : null;

    public void Add(EntityConfiguration mapping) =>
        _mappings.Add(mapping.EntityType, mapping);

    public void Configure() =>
        _mappings = _mappings.AsReadOnly();
}
