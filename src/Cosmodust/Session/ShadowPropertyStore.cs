using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Cosmodust.Session;

/// <summary>
/// An in-memory store for storing shadow properties of entities. This class is thread-safe.
/// </summary>
public sealed class ShadowPropertyStore : IDisposable
{
    public static readonly IDictionary<string, object?> EmptyShadowPropertyEntry =
        new Dictionary<string, object?>(capacity: 0).AsReadOnly();

    private readonly ConcurrentDictionary<
        object,
        IDictionary<string, object?>> _store = new();

    /// <summary>
    /// Sets the shadow property value for the specified entity and property name.
    /// </summary>
    /// <param name="entity">The entity to set the shadow property for.</param>
    /// <param name="propertyName">The name of the shadow property to set.</param>
    /// <param name="value">The value to set the shadow property to.</param>
    public void WriteSharedValue(object entity, string propertyName, object? value)
    {
        _store.AddOrUpdate(
            key: entity,
            addValueFactory: _ => new Dictionary<string, object?> { { propertyName, value } },
            updateValueFactory: (_, existingProperties) => {
                existingProperties[propertyName] = value;
                return existingProperties;
            });
    }

    public object? ReadSharedValue(object entity, string propertyName)
    {
        return _store.TryGetValue(entity, out var shadowProperties)
            ? shadowProperties.TryGetValue(propertyName, out var value)
                ? value
                : null
            : null;
    }

    /// <summary>
    /// Removes the given entity from the store and returns its associated shadow properties.
    /// </summary>
    /// <param name="entity">The entity to remove from the cache.</param>
    /// <returns>The shadow properties associated with the entity, or an empty dictionary if none were found.</returns>
    public IDictionary<string, object?> Borrow(object entity)
    {
        _store.TryRemove(entity, out var shadowProperties);

        return shadowProperties ?? new Dictionary<string, object?>();
    }

    public void Return(object entity, IDictionary<string, object?> shadowProperties)
    {
        var addResult = _store.TryAdd(entity, shadowProperties);

        Debug.Assert(addResult, "Failed to give shadow property ownership.");
    }

    public void Clear() =>
        _store.Clear();

    public void Dispose() =>
        _store.Clear();
}
