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
    public required ShadowPropertyStore Store { get; init; }
    public IDictionary<string, object?> ShadowPropertyValues { get; private set; }
        = ShadowPropertyStore.EmptyShadowPropertyEntry;
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

    public TProperty? ReadShadowProperty<TProperty>(string shadowPropertyName) =>
        ShadowPropertyValues.TryGetValue(shadowPropertyName, out var shadowPropertyValue)
            ? (TProperty?) shadowPropertyValue
            : default;

    public void WriteShadowProperty<TProperty>(string shadowPropertyName, TProperty? value) =>
        ShadowPropertyValues[shadowPropertyName] = value;

    public void WriteShadowProperty(string shadowPropertyName, object? value) =>
        ShadowPropertyValues[shadowPropertyName] = value;

    /// <summary>
    /// Borrows shadow properties from the store for the current entity.
    /// </summary>
    public void BorrowShadowPropertiesFromStore()
    {
        Debug.Assert(Entity != null);
        ShadowPropertyValues = Store.Borrow(Entity);
        Debug.WriteLine($"Retrieved entity '{Id}' from the shadow property store.");
    }

    /// <summary>
    /// Returns the shadow properties of the entity to the store for JSON serialization.
    /// </summary>
    public void ReturnShadowPropertiesToStore()
    {
        Debug.Assert(Entity != null);

        try
        {
            Store.Return(Entity, ShadowPropertyValues);
            Debug.WriteLine($"Returned entity '{Id}' to the shadow property store.");
        }

        finally
        {
            ShadowPropertyValues = ShadowPropertyStore.EmptyShadowPropertyEntry;
        }
    }
}
