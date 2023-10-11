namespace CosmicConnector;

public sealed class ChangeTracker
{
    private readonly List<EntityEntry> _entries = new();

    public ChangeTracker(EntityConfigurationHolder entityConfiguration)
    {
        ArgumentNullException.ThrowIfNull(entityConfiguration);

        EntityConfiguration = entityConfiguration;
    }

    public EntityConfigurationHolder EntityConfiguration { get; }
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

    private EntityEntry CreateEntry(object entity)
    {
        var config = EntityConfiguration.Get(entity.GetType()) ?? throw new InvalidOperationException($"No configuration has been registered for type {entity.GetType().FullName}.");

        var entry = new EntityEntry
        {
            Id = config.IdSelector.GetString(entity),
            ContainerName = config.ContainerName,
            PartitionKey = config.PartitionKeySelector.GetString(entity),
            Entity = entity,
            EntityType = entity.GetType()
        };

        _entries.Add(entry);

        return entry;
    }
}
