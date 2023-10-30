using System.Diagnostics;
using Cosmodust.Session;
using Cosmodust.Store;

namespace Cosmodust.Tracking;

public sealed class ChangeTracker : IDisposable
{
    private readonly List<EntityEntry> _entries = new();
    private readonly Dictionary<(Type Type, string Id), object> _entityByTypeId = new();
    private readonly Dictionary<object, EntityEntry> _entriesByEntity = new();

    public ChangeTracker(
        EntityConfigurationProvider entityConfiguration,
        ShadowPropertyStore shadowPropertyStore)
    {
        ArgumentNullException.ThrowIfNull(entityConfiguration);
        ArgumentNullException.ThrowIfNull(shadowPropertyStore);

        EntityConfiguration = entityConfiguration;
        ShadowPropertyStore = shadowPropertyStore;
    }

    public EntityConfigurationProvider EntityConfiguration { get; }
    public ShadowPropertyStore ShadowPropertyStore { get; }
    public IReadOnlyList<EntityEntry> Entries => _entries;

    public IEnumerable<EntityEntry> PendingChanges =>
        _entries.Where(x => x.State != EntityState.Unchanged);

    public EntityEntry? Entry(object entity) =>
        _entriesByEntity.TryGetValue(key: entity, out var entry)
            ? entry
            : default;

    /// <summary>
    /// Registers an entity as added in the change tracker.
    /// </summary>
    /// <param name="entity">The entity to register.</param>
    public void RegisterAdded(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        CreateEntry(entity, EntityState.Added);
    }

    /// <summary>
    /// Registers an entity as unchanged in the change tracker.
    /// </summary>
    /// <param name="entity">The entity to register.</param>
    public void RegisterUnchanged(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        CreateEntry(entity, EntityState.Unchanged);
    }

    public object GetOrRegisterUnchanged(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var config = EntityConfiguration.Get(entity.GetType());
        var id = config.IdSelector.GetString(entity);

        if (_entityByTypeId.TryGetValue((Type: entity.GetType(), Id: id), out var trackedEntity))
            return trackedEntity;

        RegisterUnchanged(entity);

        return entity;
    }

    /// <summary>
    /// Registers an entity as modified in the change tracker.
    /// </summary>
    /// <param name="entity">The entity to register as modified.</param>
    public void RegisterModified(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = Entry(entity)
                    ?? throw new InvalidOperationException($"Cannot update entity of type {entity.GetType()} because it has not been loaded into the session.");

        entry.Modify();
    }

    /// <summary>
    /// Registers an entity to be removed from the session.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void RegisterRemoved(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = Entry(entity)
                    ?? throw new InvalidOperationException($"Cannot update entity of type {entity.GetType()} because it has not been loaded into the session.");

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
        if (_entityByTypeId.TryGetValue((Type: typeof(TEntity), Id: id), out var value))
        {
            entity = (TEntity?) value;
            return true;
        }

        entity = default;
        return false;
    }

    public bool Exists<TEntity>(string id) =>
        _entityByTypeId.ContainsKey((Type: typeof(TEntity), Id: id));

    /// <summary>
    /// Commits all changes made to the tracked entities by removing all entities
    /// marked as removed and unchanging all entities marked as added or modified.
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
                    OnRemove(entry);
                    i--;
                    break;
                case EntityState.Unchanged:
                    break;
            }

            // now that all writes are finished, borrow
            entry.BorrowShadowPropertiesFromStore();
        }
    }

    private void OnRemove(EntityEntry entry)
    {
        entry.Detach();
        _entries.Remove(entry);
        _entityByTypeId.Remove((Type: entry.EntityType, entry.Id));
    }

    private EntityEntry CreateEntry(object entity, EntityState state)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entityType = entity.GetType();

        var config = EntityConfiguration.Get(entityType);
        var id = config.IdSelector.GetString(entity);

        if (_entityByTypeId.ContainsKey((Type: entityType, Id: id)))
            throw new InvalidOperationException($"An entity of type '{entityType.Name}' with ID '{id}' has already been loaded into the session.");

        var entry = new EntityEntry
        {
            Id = id,
            ContainerName = config.ContainerName,
            PartitionKey = config.PartitionKeySelector.GetString(entity),
            Entity = entity,
            EntityType = entity.GetType(),
            Store = ShadowPropertyStore,
            State = state
        };

        entry.BorrowShadowPropertiesFromStore();

        if(state == EntityState.Added)
            foreach (var shadowProperty in config.ShadowProperties)
                entry.WriteShadowProperty(shadowProperty.PropertyName, value: shadowProperty.DefaultValue);

        _entries.Add(entry);
        _entityByTypeId.Add((Type: entry.EntityType, entry.Id), entity);
        _entriesByEntity.Add(key: entity, value: entry);

        return entry;
    }

    public void Dispose()
    {
        foreach (var entry in _entries)
        {
            try
            {
                entry.BorrowShadowPropertiesFromStore();
            }

            catch
            {
                // ignored
            }
        }
    }
}
