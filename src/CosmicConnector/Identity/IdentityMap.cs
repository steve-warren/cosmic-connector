namespace CosmicConnector;

/// <summary>
/// Represents a map of entities by their unique identifier and type.
/// </summary>
public sealed class IdentityMap
{
    private readonly Dictionary<(Type Type, string Id), object?> _entities = new();

    public IdentityMap(EntityConfigurationHolder entityConfiguration)
    {
        EntityConfiguration = entityConfiguration;
    }

    public EntityConfigurationHolder EntityConfiguration { get; }

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

    public void Attach<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);
        Attach(id, entity);
    }

    /// <summary>
    /// Adds or updates an entity in the identity map.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being added or updated.</typeparam>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="entity">The entity to add or update.</param>
    public void Attach<TEntity>(string id, TEntity? entity)
    {
        _entities.Add((Type: typeof(TEntity), Id: id), entity);
    }

    public bool Exists<TEntity>(string id) => _entities.ContainsKey((Type: typeof(TEntity), Id: id));

    public void EnsureExists<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);

        if (!_entities.TryGetValue((Type: typeof(TEntity), Id: id), out _))
            throw new InvalidOperationException($"The entity of type '{typeof(TEntity).Name}' with ID '{id}' does not exist in the identity map.");
    }

    /// <summary>
    /// Detaches the entity with the specified type and ID from the identity map.
    /// </summary>
    /// <param name="entityType">The type of the entity to detach.</param>
    /// <param name="id">The ID of the entity to detach.</param>
    /// <returns><c>true</c> if the entity was detached successfully; otherwise, <c>false</c>.</returns>
    public bool Detatch(Type entityType, string id) => _entities.Remove((Type: entityType, Id: id));

    private string GetId<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var config = EntityConfiguration.Get(typeof(TEntity)) ?? throw new InvalidOperationException($"No ID accessor has been registered for type {typeof(TEntity).FullName}.");

        return config.IdSelector.GetString(entity);
    }
}
