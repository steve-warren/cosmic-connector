using System.Diagnostics.CodeAnalysis;

namespace Cosmodust.Store;

public class EntityConfigurationProvider
{
    private IDictionary<Type, EntityConfiguration> _mappings =
        new Dictionary<Type, EntityConfiguration>();

    public EntityConfiguration Get(Type entityType) =>
        _mappings.TryGetValue(entityType, out var mapping)
            ? mapping
            : throw new InvalidOperationException($"No configuration has been registered for type {entityType.FullName}.");

    public bool TryGetEntityConfiguration(
        Type entityType,
        [MaybeNullWhen(false)] out EntityConfiguration entityConfiguration) =>
        _mappings.TryGetValue(entityType, out entityConfiguration);

    public void AddEntityConfiguration(EntityConfiguration mapping) =>
        _mappings.Add(mapping.EntityType, mapping);

    public void Configure() =>
        _mappings = _mappings.AsReadOnly();
}
