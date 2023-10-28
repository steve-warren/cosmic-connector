namespace Cosmodust.Tracking;

public sealed class EntityEntry
{
    public required string Id { get; init; }
    public required string ContainerName { get; init; }
    public required string PartitionKey { get; init; }
    public required object Entity { get; init; }
    public required Type EntityType { get; init; }
    public required IDictionary<string, object> ShadowProperties { get; init; }
    public EntityState State { get; private set; } = EntityState.Detached;

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

    public TProperty? Property<TProperty>(string shadowPropertyName) =>
        ShadowProperties.TryGetValue(shadowPropertyName, out var shadowPropertyValue)
            ? (TProperty) shadowPropertyValue
            : default;
}
