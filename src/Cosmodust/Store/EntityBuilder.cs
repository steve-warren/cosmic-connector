using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Shared;

namespace Cosmodust.Store;

/// <summary>
/// A builder class for creating entity configurations for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of entity to configure.</typeparam>
public class EntityBuilder<TEntity> : IEntityBuilder where TEntity : class
{
    private readonly ShadowPropertyStore _shadowPropertyStore;
    private EntityConfiguration _entityConfiguration = new(typeof(TEntity));
    private readonly HashSet<FieldAccessor> _fields = new();
    private readonly HashSet<PropertyAccessor> _properties = new();
    private readonly HashSet<ShadowProperty> _shadowProperties = new();

    public EntityBuilder(ShadowPropertyStore shadowPropertyStore)
    {
        Ensure.NotNull(shadowPropertyStore);

        _shadowPropertyStore = shadowPropertyStore;
    }

    /// <summary>
    /// Configures the entity to use the specified function to extract the ID value.
    /// </summary>
    /// <param name="idSelector">A function that extracts the ID value from an entity instance.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> HasId(Func<TEntity, string> idSelector)
    {
        Ensure.NotNull(idSelector);

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
        Ensure.NotNull(partitionKeySelector);

        _entityConfiguration = _entityConfiguration with
        {
            PartitionKeySelector = new StringSelector<TEntity>(partitionKeySelector)
        };

        return this;
    }

    public EntityBuilder<TEntity> HasPartitionKey(
        Func<TEntity, string> partitionKeySelector,
    string partitionKeyName)
    {
        Ensure.NotNull(partitionKeySelector);
        Ensure.NotNullOrWhiteSpace(partitionKeyName);

        _entityConfiguration = _entityConfiguration with
        {
            PartitionKeyName = partitionKeyName,
            PartitionKeySelector = new StringSelector<TEntity>(partitionKeySelector)
        };

        return this;
    }

    /// <summary>
    /// Adds a field to the entity configuration.
    /// </summary>
    /// <param name="fieldName">The name of the field to add.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> HasField(string fieldName)
    {
        Ensure.NotNullOrWhiteSpace(fieldName);

        var accessor = FieldAccessor.Create(fieldName, typeof(TEntity));

        _fields.Add(accessor);

        return this;
    }

    public EntityBuilder<TEntity> HasProperty(string propertyName)
    {
        Ensure.NotNullOrWhiteSpace(propertyName);

        var accessor = PropertyAccessor.Create(propertyName, typeof(TEntity));

        _properties.Add(accessor);

        return this;
    }

    public EntityBuilder<TEntity> HasShadowProperty<TProperty>(string propertyName)
    {
        Ensure.NotNullOrWhiteSpace(propertyName);

        var shadowProperty = new ShadowProperty
        {
            PropertyType = typeof(TProperty),
            PropertyName = propertyName,
            Store = _shadowPropertyStore
        };

        _shadowProperties.Add(shadowProperty);

        return this;
    }

    /// <summary>
    /// Sets the name of the container where the entity will be stored.
    /// </summary>
    /// <param name="containerName">The name of the container.</param>
    public void ToContainer(string containerName)
    {
        Ensure.NotNullOrWhiteSpace(containerName);

        _entityConfiguration = _entityConfiguration with { ContainerName = containerName };
    }

    EntityConfiguration IEntityBuilder.Build()
    {
        return _entityConfiguration = _entityConfiguration with
        {
            Fields = _fields,
            Properties = _properties,
            ShadowProperties = _shadowProperties
        };
    }
}
