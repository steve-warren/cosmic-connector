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
    private readonly HashSet<JsonProperty> _jsonProperties = new();

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
            throw new ArgumentException("Invalid id eventsEnumerableSelector. Expected a property eventsEnumerableSelector.");
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
            throw new ArgumentException("Invalid partition key eventsEnumerableSelector. Expected a property eventsEnumerableSelector.");
        }

        return this;
    }

    /// <summary>
    /// Sets the partition key for the entity using the specified partition key selector and partition key name.
    /// </summary>
    /// <param name="partitionKeySelector">The function that selects the partition key from the entity.</param>
    /// <param name="partitionKeyName">The name of the partition key.</param>
    /// <returns>The updated instance of the <see cref="EntityBuilder{TEntity}"/> class.</returns>
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

    /// <summary>
    /// Adds a property to the entity builder.
    /// </summary>
    /// <param name="propertyName">The name of the property to add.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> WithProperty(string propertyName)
    {
        Ensure.NotNullOrWhiteSpace(propertyName);

        var accessor = PropertyAccessor.Create(propertyName, typeof(TEntity));

        _properties.Add(accessor);

        return this;
    }

    /// <summary>
    /// Adds a JSON property to the entity builder.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> WithJsonProperty<TProperty>(string propertyName)
    {
        Ensure.NotNullOrWhiteSpace(propertyName);

        var jsonProperty = new JsonProperty
        {
            PropertyType = typeof(TProperty),
            PropertyName = propertyName,
            Broker = _jsonPropertyBroker
        };

        _jsonProperties.Add(jsonProperty);

        return this;
    }

    /// <summary>
    /// Sets the domain event configuration for the entity being built.
    /// </summary>
    /// <param name="domainEventCollectionFieldName">The name of the field that stores the domain event collection.</param>
    /// <param name="eventIdFactory">A function that generates the event ID.</param>
    /// <returns>The entity builder instance.</returns>
    public EntityBuilder<TEntity> WithDomainEvents(
        string domainEventCollectionFieldName,
        Func<string> eventIdFactory)
    {
        _entityConfiguration = _entityConfiguration with
        {
            DomainEventAccessor = DomainEventAccessor.Create<TEntity>(
                domainEventCollectionFieldName,
                eventIdFactory)
        };

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
            JsonProperties = _jsonProperties
        };
    }
}
