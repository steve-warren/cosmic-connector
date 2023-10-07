namespace CosmicConnector;

public sealed class ChangeTracker
{
    private readonly List<EntityEntry> _entries = new();

    public ChangeTracker(IdentityAccessor identityAccessor)
    {
        ArgumentNullException.ThrowIfNull(identityAccessor);

        IdentityAccessor = identityAccessor;
    }

    public IdentityAccessor IdentityAccessor { get; }
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

    /// <summary>
    /// Registers an entity as added in the change tracker.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="entity">The entity to register.</param>
    public void RegisterAdded(object entity)
    {
        var entry = CreateEntry(entity);

        entry.Add();

        _entries.Add(entry);
    }

    /// <summary>
    /// Registers an entity as unchanged in the change tracker.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="entity">The entity to register.</param>
    public void RegisterUnchanged(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        var entry = CreateEntry(entity);

        entry.Unchange();

        _entries.Add(entry);
    }

    /// <summary>
    /// Registers an entity as modified in the change tracker.
    /// </summary>
    /// <param name="entity">The entity to register as modified.</param>
    public void RegisterModified(object entity)
    {
        var entry = FindEntry(entity) ?? throw new InvalidOperationException($"Cannot update entity of type {entity.GetType()} because it has not been loaded into the session.");

        entry.Modify();
    }

    /// <summary>
    /// Registers an entity to be removed from the session.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void RegisterRemoved(object entity)
    {
        var entry = FindEntry(entity) ?? throw new InvalidOperationException($"Cannot update entity of type {entity.GetType()} because it has not been loaded into the session.");

        entry.Remove();
    }

    /// <summary>
    /// Commits all changes made to the tracked entities by removing all entities marked as removed and unchanging all entities marked as added or modified.
    /// </summary>
    public void Commit()
    {
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];

            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    entry.Unchange();
                    break;

                case EntityState.Removed:
                    entry.Detach();
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

    private EntityEntry CreateEntry(object entity) => new()
    {
        Id = IdentityAccessor.GetId(entity),
        PartitionKey = IdentityAccessor.GetId(entity),
        Entity = entity,
        EntityType = entity.GetType()
    };
}
