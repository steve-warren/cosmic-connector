namespace CosmicConnector;

public class EntityConfigurationHolder
{
    private readonly Dictionary<Type, EntityConfiguration> _mappings = new();

    public EntityConfiguration? Get(Type entityType)
    {
        if (_mappings.TryGetValue(entityType, out var mapping))
            return mapping;

        return null;
    }

    public void Add(EntityConfiguration mapping) => _mappings.Add(mapping.EntityType, mapping);
}
