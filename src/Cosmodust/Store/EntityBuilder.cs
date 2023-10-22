using System.Linq.Expressions;
using Cosmodust.Serialization;

namespace Cosmodust.Store;

/// <summary>
/// A builder class for creating entity configurations for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of entity to configure.</typeparam>
public class EntityBuilder<TEntity> : IEntityBuilder where TEntity : class
{
    private EntityConfiguration _entityConfiguration = new(typeof(TEntity));
    private readonly HashSet<FieldAccessor> _fields = new();
    private readonly HashSet<PropertyAccessor> _properties = new();

    /// <summary>
    /// Configures the entity to use the specified function to extract the ID value.
    /// </summary>
    /// <param name="idSelector">A function that extracts the ID value from an entity instance.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> HasId(Func<TEntity, string> idSelector)
    {
        _entityConfiguration = _entityConfiguration with { IdSelector = new StringSelector<TEntity>(idSelector) };

        return this;
    }

    /// <summary>
    /// Configures the partition key for the entity.
    /// </summary>
    /// <param name="partitionKeySelector">A function that selects the partition key for the entity.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> HasPartitionKey(Func<TEntity, string> partitionKeySelector)
    {
        _entityConfiguration = _entityConfiguration with { PartitionKeySelector = new StringSelector<TEntity>(partitionKeySelector) };

        return this;
    }

    /// <summary>
    /// Adds a field to the entity configuration.
    /// </summary>
    /// <param name="name">The name of the field to add.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> HasField(string name)
    {
        var accessor = FieldAccessor.Create(name, typeof(TEntity));

        _fields.Add(accessor);

        return this;
    }

    public EntityBuilder<TEntity> HasProperty(Expression<Func<TEntity, object>> propertySelector)
    {
        var accessor = PropertyAccessor.Create(propertySelector);

        _properties.Add(accessor);

        return this;
    }

    public EntityBuilder<TEntity> HasProperty(string name)
    {
        var accessor = PropertyAccessor.Create(name, typeof(TEntity));

        _properties.Add(accessor);

        return this;
    }

    /// <summary>
    /// Sets the name of the container where the entity will be stored.
    /// </summary>
    /// <param name="containerName">The name of the container.</param>
    public void ToContainer(string containerName)
    {
        _entityConfiguration = _entityConfiguration with { ContainerName = containerName };
    }

    EntityConfiguration IEntityBuilder.Build()
    {
        return _entityConfiguration = _entityConfiguration with
        {
            Fields = _fields,
            Properties = _properties
        };
    }
}
