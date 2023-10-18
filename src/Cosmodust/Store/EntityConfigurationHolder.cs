using System.Collections.ObjectModel;

namespace Cosmodust;

public class EntityConfigurationHolder
{
    private IDictionary<Type, EntityConfiguration> _mappings;

    public EntityConfigurationHolder()
    {
        _mappings = new Dictionary<Type, EntityConfiguration>();
    }

    public EntityConfiguration? Get(Type entityType)
    {
        if (_mappings.TryGetValue(entityType, out var mapping))
            return mapping;

        return null;
    }

    public void Add(EntityConfiguration mapping)
    {
        _mappings.Add(mapping.EntityType, mapping);
    }

    public void Clear()
    {
        _mappings.Clear();
    }

    public void Configure()
    {
        _mappings = _mappings.AsReadOnly();
    }
}
