using Cosmodust.Store;

namespace Cosmodust.Tracking;

public sealed class ChangeTracker
{
    private readonly List<EntityEntry> _entries = new();
    private readonly Dictionary<(Type Type, string Id), object?> _entities = new();
    private readonly EntityConfigurationHolder _entityConfiguration;

    public ChangeTracker(EntityConfigurationHolder entityConfiguration)
    {
        ArgumentNullException.ThrowIfNull(entityConfiguration);
        _entityConfiguration = entityConfiguration;
    }

    public IReadOnlyList<EntityEntry> Entries => _entries;

    public IEnumerable<EntityEntry> PendingChanges =>
        _entries.Where(x => x.State != EntityState.Unchanged);

    private EntityEntry? FindEntry(object entity) =>
        _entries.FirstOrDefault(x => x.Entity == entity);

    /// <summary>
    /// Registers an entity as added in the change tracker.
    /// </summary>
    /// <param name="entity">The entity to register.</param>
    public void RegisterAdded(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = CreateEntry(entity);

        entry.Add();
    }

    /// <summary>
    /// Registers an entity as unchanged in the change tracker.
    /// </summary>
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
        ArgumentNullException.ThrowIfNull(entity);

        var entry = FindEntry(entity) ?? throw new InvalidOperationException($"Cannot update entity of type {entity.GetType()} because it has not been loaded into the session.");

        entry.Modify();
    }

    /// <summary>
    /// Registers an entity to be removed from the session.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void RegisterRemoved(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = FindEntry(entity) ?? throw new InvalidOperationException($"Cannot update entity of type {entity.GetType()} because it has not been loaded into the session.");

        entry.Remove();
    }

    /// <summary>
    /// Attempts to retrieve an entity of type <typeparamref name="TEntity"/> with the specified ID from the identity map.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="entity">When this method returns, contains the entity associated with the specified ID, if the ID is found; otherwise, the default value for the type of the <paramref name="entity"/> parameter. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the identity map contains an entity with the specified ID; otherwise, <c>false</c>.</returns>
    public bool TryGet<TEntity>(string id, out TEntity? entity)
    {
        if (_entities.TryGetValue((Type: typeof(TEntity), Id: id), out var value))
        {
            entity = (TEntity?) value;
            return true;
        }

        entity = default;
        return false;
    }

    public bool Exists<TEntity>(string id) => _entities.ContainsKey((Type: typeof(TEntity), Id: id));

    public void EnsureExists<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);

        if (!_entities.TryGetValue((Type: typeof(TEntity), Id: id), out _))
            throw new InvalidOperationException($"The entity of type '{typeof(TEntity).Name}' with ID '{id}' does not exist in the identity map.");
    }

    private string GetId<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var config = _entityConfiguration.Get(typeof(TEntity)) ?? throw new InvalidOperationException($"No ID accessor has been registered for type {typeof(TEntity).FullName}.");

        return config.IdSelector.GetString(entity);
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
                    _entities.Remove((Type: entry.EntityType, entry.Id));
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
        ArgumentNullException.ThrowIfNull(entity);

        var entityType = entity.GetType();

        var config = _entityConfiguration.Get(entityType) ?? throw new InvalidOperationException($"No configuration has been registered for type {entityType.FullName}.");
        var id = config.IdSelector.GetString(entity);

        if (_entities.ContainsKey((Type: entityType, Id: id)))
            throw new InvalidOperationException($"An entity of type '{entityType.Name}' with ID '{id}' has already been loaded into the session.");

        var entry = new EntityEntry
        {
            Id = id,
            ContainerName = config.ContainerName,
            PartitionKey = config.PartitionKeySelector.GetString(entity),
            Entity = entity,
            EntityType = entity.GetType()
        };

        _entries.Add(entry);
        _entities.Add((Type: entry.EntityType, entry.Id), entity);

        return entry;
    }
}
