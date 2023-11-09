using System.Linq.Expressions;
using System.Reflection;
using Cosmodust.Serialization;
using Cosmodust.Shared;
using Cosmodust.Tracking;

namespace Cosmodust.Store;

/// <summary>
/// A builder class for creating entity configurations for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of entity to configure.</typeparam>
public class EntityBuilder<TEntity> : IEntityBuilder where TEntity : class
{
    private readonly JsonPropertyBroker _jsonPropertyBroker;
    private EntityConfiguration _entityConfiguration = new(typeof(TEntity));
    private readonly HashSet<FieldAccessor> _fields = new();
    private readonly HashSet<PropertyAccessor> _properties = new();
    private readonly HashSet<JsonProperty> _shadowProperties = new();

    public EntityBuilder(JsonPropertyBroker jsonPropertyBroker)
    {
        Ensure.NotNull(jsonPropertyBroker);

        _jsonPropertyBroker = jsonPropertyBroker;
    }

    /// <summary>
    /// Configures the entity to use the specified function to extract the ID value.
    /// </summary>
    /// <param name="idSelector">A function that extracts the ID value from an entity instance.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> WithId(Expression<Func<TEntity, string>> idSelector)
    {
        Ensure.NotNull(idSelector);
        
        if (idSelector.Body is MemberExpression { Member: PropertyInfo propertyInfo })
        {
            var propertyName = propertyInfo.Name;
            var func = idSelector.Compile();

            _entityConfiguration = _entityConfiguration with
            {
                IsIdPropertyDefinedInEntity = string.Equals(
                    "id",
                    propertyName,
                    StringComparison.InvariantCultureIgnoreCase),
                IdPropertyName = propertyName,
                IdSelector = new StringSelector<TEntity>(func)
            };
        }

        else
        {
            throw new ArgumentException("Invalid id selector. Expected a property selector.");
        }

        return this;
    }

    /// <summary>
    /// Configures the partition key for the entity.
    /// </summary>
    /// <param name="partitionKeySelector">A function that selects the partition key for the entity.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> WithPartitionKey(Expression<Func<TEntity, string>> partitionKeySelector)
    {
        Ensure.NotNull(partitionKeySelector);

        if (partitionKeySelector.Body is MemberExpression { Member: PropertyInfo propertyInfo })
        {
            var propertyName = propertyInfo.Name;
            var func = partitionKeySelector.Compile();

            _entityConfiguration = _entityConfiguration with
            {
                IsPartitionKeyDefinedInEntity = true,
                PartitionKeyName = propertyName,
                PartitionKeySelector = new StringSelector<TEntity>(func)
            };
        }
        else
        {
            throw new ArgumentException("Invalid partition key selector. Expected a property selector.");
        }

        return this;
    }

    public EntityBuilder<TEntity> WithPartitionKey(
        Func<TEntity, string> partitionKeySelector,
    string partitionKeyName)
    {
        Ensure.NotNull(partitionKeySelector);
        Ensure.NotNullOrWhiteSpace(partitionKeyName);

        _entityConfiguration = _entityConfiguration with
        {
            IsPartitionKeyDefinedInEntity = false,
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
    public EntityBuilder<TEntity> WithField(string fieldName)
    {
        Ensure.NotNullOrWhiteSpace(fieldName);

        var accessor = FieldAccessor.Create(fieldName, typeof(TEntity));

        _fields.Add(accessor);

        return this;
    }

    public EntityBuilder<TEntity> WithProperty(string propertyName)
    {
        Ensure.NotNullOrWhiteSpace(propertyName);

        var accessor = PropertyAccessor.Create(propertyName, typeof(TEntity));

        _properties.Add(accessor);

        return this;
    }

    public EntityBuilder<TEntity> WithShadowProperty<TProperty>(string propertyName)
    {
        Ensure.NotNullOrWhiteSpace(propertyName);

        var shadowProperty = new JsonProperty
        {
            PropertyType = typeof(TProperty),
            PropertyName = propertyName,
            Broker = _jsonPropertyBroker
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
            JsonProperties = _shadowProperties
        };
    }
}
