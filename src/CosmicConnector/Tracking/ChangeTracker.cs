namespace CosmicConnector;

public sealed class ChangeTracker
{
    private readonly List<EntityEntry> _entries = new();

    public IReadOnlyList<EntityEntry> Entries => _entries;

    public EntityEntry? FindEntry(object entity) =>
        _entries.FirstOrDefault(x => x.Entity == entity);

    /// <summary>
    /// Tracks changes made to an entity with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the entity to track.</param>
    /// <param name="entity">The entity to track.</param>
    public void Track(string id, object entity) =>
        Register(id, entity, EntityState.Added);

    private void Register(string id, object entity, EntityState state)
    {
        var entry = new EntityEntry
        {
            Id = id,
            Entity = entity,
            EntityType = entity.GetType(),
            State = state
        };

        _entries.Add(entry);
    }

    public void Clear()
    {
        _entries.Clear();
    }
}
