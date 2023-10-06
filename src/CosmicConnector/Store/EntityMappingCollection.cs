namespace CosmicConnector;

public class EntityMappingCollection
{
    private readonly Dictionary<Type, EntityMapping> _mappings = new();

    public EntityMapping? Get(Type entityType)
    {
        if (_mappings.TryGetValue(entityType, out var mapping))
            return mapping;

        return null;
    }

    public void Add(EntityMapping mapping) => _mappings.Add(mapping.EntityType, mapping);
}
