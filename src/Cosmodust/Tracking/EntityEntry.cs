using System.Diagnostics;
using Cosmodust.Session;
using Cosmodust.Store;

namespace Cosmodust.Tracking;

public sealed class EntityEntry
{
    public required string Id { get; init; }
    public required string ContainerName { get; init; }
    public required string PartitionKey { get; init; }
    public required string PartitionKeyName { get; init; }
    public required object Entity { get; init; }
    public required Type EntityType { get; init; }
    public required JsonPropertyBroker Broker { get; init; }
    public required DomainEventAccessor DomainEventAccessor { get; init; }
    public string? ETag { get; set; }
    public IDictionary<string, object?> JsonProperties { get; private set; }
        = new Dictionary<string, object?>();
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
        JsonProperties["_etag"] = eTag;
    }

    public TProperty? ReadJsonProperty<TProperty>(string jsonPropertyName) =>
        JsonProperties.TryGetValue(jsonPropertyName, out var value)
            ? (TProperty?) value
            : default;

    public void WriteJsonProperty<TProperty>(string jsonPropertyName, TProperty? value) =>
        JsonProperties[jsonPropertyName] = value;

    public void WriteJsonProperty(string jsonPropertyName, object? value) =>
        JsonProperties[jsonPropertyName] = value;

    /// <summary>
    /// Reads JSON properties from the broker for the current entity.
    /// </summary>
    public void RemoveJsonPropertiesFromBroker()
    {
        Debug.Assert(Entity != null);
        JsonProperties = Broker.RemoveEntityProperties(Entity) ?? JsonProperties;
        Debug.WriteLine($"Retrieved entity '{Id}' from the shadow property broker.");
    }

    /// <summary>
    /// Writes the JSON properties of the entity to the broker for serialization.
    /// </summary>
    public void AddJsonPropertiesToBroker()
    {
        Debug.Assert(Entity != null);

        try
        {
            Broker.AddEntityProperties(Entity, JsonProperties);
            Debug.WriteLine($"Returned entity '{Id}' to the shadow property broker.");
        }

        finally
        {
            JsonProperties = JsonPropertyBroker.EmptyJsonProperties;
        }
    }
}
