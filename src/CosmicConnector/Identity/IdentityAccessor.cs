using System.Linq.Expressions;

namespace CosmicConnector;

/// <summary>
/// Provides a mechanism for accessing the ID of an entity of a specified type.
/// </summary>
public sealed class IdentityAccessor
{
    private readonly Dictionary<Type, Func<object, string>> _accessors = new();

    /// <summary>
    /// Configures the entity ID accessor for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to configure the ID accessor for.</typeparam>
    public void Register<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (_accessors.ContainsKey(entityType))
            return;

        _accessors[entityType] = CreateAccessor(entityType);
    }

    /// <summary>
    /// Ensures that an ID accessor has been registered for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to check for a registered ID accessor.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if no ID accessor has been registered for the specified entity type.</exception>
    public void EnsureRegistered<TEntity>() where TEntity : class
    {
        if (!_accessors.ContainsKey(typeof(TEntity)))
            throw new InvalidOperationException($"No ID accessor has been registered for type {typeof(TEntity).FullName}.");
    }

    /// <summary>
    /// Gets the ID of the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity.</param>
    /// <returns>The ID of the entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no ID accessor has been registered for the specified entity type.</exception>
    public string GetId<TEntity>(TEntity entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        if (_accessors.TryGetValue(typeof(TEntity), out var accessor))
            return accessor(entity);

        throw new InvalidOperationException($"No ID accessor has been registered for type {typeof(TEntity).FullName}.");
    }

    private static Func<object, string> CreateAccessor(Type entityType)
    {
        var property = entityType.GetProperty("Id");

        if (property is null || property.PropertyType != typeof(string))
            throw new InvalidOperationException($"The type {entityType.FullName} does not have a property named 'Id' of type string.");

        var parameter = Expression.Parameter(typeof(object), "entity");
        var cast = Expression.Convert(parameter, entityType);
        var propertyAccess = Expression.Property(cast, property);
        var lambda = Expression.Lambda<Func<object, string>>(propertyAccess, parameter);

        return lambda.Compile();
    }
}
