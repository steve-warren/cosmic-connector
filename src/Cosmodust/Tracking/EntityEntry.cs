using System.Diagnostics;
using Cosmodust.Session;

namespace Cosmodust.Tracking;

public sealed class EntityEntry
{
    public required string Id { get; init; }
    public required string ContainerName { get; init; }
    public required string PartitionKey { get; init; }
    public required object Entity { get; init; }
    public required Type EntityType { get; init; }
    public required JsonSerializerPropertyStore Store { get; init; }
    public string? ETag { get; set; }
    public IDictionary<string, object?> JsonPropertyValues { get; private set; }
        = JsonSerializerPropertyStore.EmptyEntityPropertyEntry;
    public EntityState State { get; set; } = EntityState.Detached;

    public bool IsModified => State == EntityState.Modified;
    public bool IsRemoved => State == EntityState.Removed;
    public bool IsUnchanged => State == EntityState.Unchanged;
    public bool IsAdded => State == EntityState.Added;
    public bool IsPendingChanges => State != EntityState.Unchanged;

    /// <summary>
    /// Marks the entity as added by setting the state to <see cref="EntityState.Added"/>.
    /// </summary>
    public void Add() =>
        State = EntityState.Added;

    /// <summary>
    /// Marks the entity as modified by setting the state to <see cref="EntityState.Modified"/>.
    /// </summary>
    public void Modify() =>
        State = EntityState.Modified;

    /// <summary>
    /// Marks the entity as removed by setting the state to <see cref="EntityState.Removed"/>.
    /// </summary>
    public void Remove() =>
        State = EntityState.Removed;

    /// <summary>
    /// Marks the entity as unchanged by setting the state to <see cref="EntityState.Unchanged"/>.
    /// </summary>
    public void Unchange() =>
        State = EntityState.Unchanged;

    public void Detach() =>
        State = EntityState.Detached;

    public void UpdateETag(string eTag)
    {
        ETag = eTag;
        JsonPropertyValues["_etag"] = eTag;
    }

    public TProperty? ReadShadowProperty<TProperty>(string shadowPropertyName) =>
        JsonPropertyValues.TryGetValue(shadowPropertyName, out var shadowPropertyValue)
            ? (TProperty?) shadowPropertyValue
            : default;

    public void WriteShadowProperty<TProperty>(string shadowPropertyName, TProperty? value) =>
        JsonPropertyValues[shadowPropertyName] = value;

    public void WriteShadowProperty(string shadowPropertyName, object? value) =>
        JsonPropertyValues[shadowPropertyName] = value;

    /// <summary>
    /// Borrows shadow properties from the store for the current entity.
    /// </summary>
    public void FetchJsonPropertiesFromSerializer()
    {
        Debug.Assert(Entity != null);
        JsonPropertyValues = Store.Borrow(Entity);
        Debug.WriteLine($"Retrieved entity '{Id}' from the shadow property store.");
    }

    /// <summary>
    /// Sends the JSON properties of the entity to the store for serialization.
    /// </summary>
    public void SendJsonPropertiesToSerializer()
    {
        Debug.Assert(Entity != null);

        try
        {
            Store.Return(Entity, JsonPropertyValues);
            Debug.WriteLine($"Returned entity '{Id}' to the shadow property store.");
        }

        finally
        {
            JsonPropertyValues = JsonSerializerPropertyStore.EmptyEntityPropertyEntry;
        }
    }
}
