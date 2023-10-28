using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Cosmodust.Session;

/// <summary>
/// A cache for storing shadow properties of entities. This class is thread-safe.
/// </summary>
public sealed class ShadowPropertyCache : IDisposable
{
    private static readonly IDictionary<string, object> s_empty =
        new Dictionary<string, object>(capacity: 0).AsReadOnly();

    private readonly ConcurrentDictionary<
        object,
        Dictionary<string, object>> _cache = new();

    /// <summary>
    /// Sets the shadow property value for the specified entity and property name.
    /// </summary>
    /// <param name="entity">The entity to set the shadow property for.</param>
    /// <param name="propertyName">The name of the shadow property to set.</param>
    /// <param name="value">The value to set the shadow property to.</param>
    public void SetShadowPropertyValue(object entity, string propertyName, object value)
    {
        _cache.AddOrUpdate(
            key: entity,
            addValueFactory: _ => new Dictionary<string, object> { { propertyName, value } },
            updateValueFactory: (_, existingProperties) => {
                existingProperties[propertyName] = value;
                return existingProperties;
            });
    }

    public void Clear() =>
        _cache.Clear();

    public void Dispose() =>
        _cache.Clear();

    /// <summary>
    /// Removes the given entity from the cache and returns its associated shadow properties.
    /// </summary>
    /// <param name="entity">The entity to remove from the cache.</param>
    /// <returns>The shadow properties associated with the entity, or an empty dictionary if none were found.</returns>
    public IDictionary<string, object> TakeOwnershipOf(object entity)
    {
        _cache.TryRemove(entity, out var shadowProperties);

        return shadowProperties ?? s_empty;
    }
}
