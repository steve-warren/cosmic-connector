namespace CosmicConnector;

public sealed class ChangeTracker
{
    private readonly List<EntityEntry> _entries = new();

    public IReadOnlyList<EntityEntry> Entries => _entries;

    /// <summary>
    /// Gets an enumerable collection of <see cref="EntityEntry"/> objects that represent entities
    /// that have been marked for deletion from the database.
    /// </summary>
    public IEnumerable<EntityEntry> RemovedEntries =>
        _entries.Where(x => x.State == EntityState.Removed);

    public IEnumerable<EntityEntry> PendingChanges =>
        _entries.Where(x => x.State != EntityState.Unchanged);

    public EntityEntry? FindEntry(object entity) =>
        _entries.FirstOrDefault(x => x.Entity == entity);

    public int Count => _entries.Count;

    /// <summary>
    /// Tracks changes made to an entity with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the entity to track.</param>
    /// <param name="entity">The entity to track.</param>
    public void TrackAdded(string id, object entity) =>
        Register(id, entity, EntityState.Added);

    public void TrackUnchanged(string id, object entity) =>
        Register(id, entity, EntityState.Unchanged);

    private void Register(string id, object entity, EntityState state)
    {
        var entry = new EntityEntry
        {
            Id = id,
            PartitionKey = id,
            Entity = entity,
            EntityType = entity.GetType(),
            State = state
        };

        _entries.Add(entry);
    }

    /// <summary>
    /// Resets the change tracker by unchanging all added and modified entities, and removing all removed entities.
    /// </summary>
    public void Reset()
    {
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Unchange();
                    break;
                case EntityState.Modified:
                    entry.Unchange();
                    break;
                case EntityState.Removed:
                    _entries.RemoveAt(i);
                    i--;
                    break;
                case EntityState.Unchanged:
                    break;
                default:
                    throw new InvalidOperationException($"Unknown entity state: {entry.State}");
            }
        }
    }
}
