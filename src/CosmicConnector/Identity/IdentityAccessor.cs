using System.Linq.Expressions;

namespace CosmicConnector;

/// <summary>
/// Provides a mechanism for accessing the ID of an entity of a specified type.
/// </summary>
public sealed class IdentityAccessor
{
    private readonly Dictionary<Type, Func<object, string>> _accessors = new();

    /// <summary>
    /// Registers a type with the identity accessor.
    /// </summary>
    /// <typeparam name="TEntity">The type to register.</typeparam>
    public void RegisterType<TEntity>()
    {
        var entityType = typeof(TEntity);

        _accessors.Add(entityType, CreateAccessor(entityType));
    }

    /// <summary>
    /// Gets the ID of the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity.</param>
    /// <returns>The ID of the entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no ID accessor has been registered for the entity type, or when the entity does not have an ID.</exception>
    public string GetId<TEntity>(TEntity entity) =>
        GetId((object) entity);

    public string GetId(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var entityType = entity.GetType();

        if (!_accessors.TryGetValue(entityType, out var accessor))
            throw new InvalidOperationException($"No ID accessor has been registered for type {entityType.FullName}.");

        var id = accessor(entity);

        return string.IsNullOrEmpty(id) ? throw new InvalidOperationException($"The entity of type '{entityType.Name}' does not have an Id.") : id;
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
