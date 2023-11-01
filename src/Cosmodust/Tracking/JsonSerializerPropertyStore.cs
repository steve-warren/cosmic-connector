using System.Collections.Concurrent;
using System.Diagnostics;

namespace Cosmodust.Tracking;

/// <summary>
/// An in-memory store for storing entity properties, such as shadow properties. This class is thread-safe.
/// </summary>
public sealed class JsonSerializerPropertyStore : IDisposable
{
    public static readonly IDictionary<string, object?> EmptyEntityPropertyEntry =
        new Dictionary<string, object?>(capacity: 0).AsReadOnly();

    private readonly ConcurrentDictionary<
        object,
        IDictionary<string, object?>> _store = new();

    /// <summary>
    /// Sets the property value for the specified entity and property name.
    /// </summary>
    /// <param name="entity">The entity to set the property for.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value to set the property to.</param>
    public void WritePropertyValue(object entity, string propertyName, object? value)
    {
        _store.AddOrUpdate(
            key: entity,
            addValueFactory: _ => new Dictionary<string, object?> { { propertyName, value } },
            updateValueFactory: (_, existingProperties) => {
                existingProperties[propertyName] = value;
                return existingProperties;
            });
    }

    public object? ReadPropertyValue(object entity, string propertyName)
    {
        return _store.TryGetValue(entity, out var shadowProperties)
            ? shadowProperties.TryGetValue(propertyName, out var value)
                ? value
                : null
            : null;
    }

    /// <summary>
    /// Removes the given entity from the store and returns its associated properties.
    /// </summary>
    /// <param name="entity">The entity to remove from the cache.</param>
    /// <returns>The properties associated with the entity, or an empty dictionary if none were found.</returns>
    public IDictionary<string, object?> Borrow(object entity)
    {
        _store.TryRemove(entity, out var properties);

        return properties ?? new Dictionary<string, object?>();
    }

    public void Return(object entity, IDictionary<string, object?> properties)
    {
        var addResult = _store.TryAdd(entity, properties);

        Debug.Assert(addResult, "Failed to give property ownership.");
    }

    public void Clear() =>
        _store.Clear();

    public void Dispose() =>
        _store.Clear();
}
