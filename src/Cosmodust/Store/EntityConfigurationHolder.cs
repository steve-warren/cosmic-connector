using System.Diagnostics.CodeAnalysis;

namespace Cosmodust.Store;

public class EntityConfigurationHolder
{
    private IDictionary<Type, EntityConfiguration> _mappings =
        new Dictionary<Type, EntityConfiguration>();

    public EntityConfiguration Get(Type entityType) =>
        _mappings.TryGetValue(entityType, out var mapping)
            ? mapping
            : throw new InvalidOperationException($"No configuration has been registered for type {entityType.FullName}.");

    public bool TryGet(
        Type entityType,
        [MaybeNullWhen(false)] out EntityConfiguration entityConfiguration) =>
        _mappings.TryGetValue(entityType, out entityConfiguration);

    public void Add(EntityConfiguration mapping) =>
        _mappings.Add(mapping.EntityType, mapping);

    public void Configure() =>
        _mappings = _mappings.AsReadOnly();
}
