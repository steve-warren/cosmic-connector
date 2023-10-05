namespace CosmicConnector;

/// <summary>
/// Represents a map of entities by their unique identifier and type.
/// </summary>
public sealed class IdentityMap
{
    private readonly Dictionary<(Type Type, string Id), object?> _entities = new();

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

    /// <summary>
    /// Adds or updates an entity in the identity map.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being added or updated.</typeparam>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="entity">The entity to add or update.</param>
    public void Put<TEntity>(string id, TEntity? entity)
    {
        _entities[(Type: typeof(TEntity), Id: id)] = entity;
    }
}
