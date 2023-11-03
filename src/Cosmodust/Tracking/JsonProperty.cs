namespace Cosmodust.Tracking;

/// <summary>
/// Represents a shadow property that can be used to broker additional data for an entity.
/// </summary>
public class JsonProperty
{
    private readonly Lazy<object?> _defaultValue;

    public JsonProperty() =>
        _defaultValue = new Lazy<object?>(valueFactory: () =>
            PropertyType == typeof(string)
                ? default
                : Activator.CreateInstance(PropertyType ?? throw new InvalidOperationException()));

    public required string PropertyName { get; init; }
    public required Type PropertyType { get; init; }
    public required JsonPropertyBroker Broker { get; init; }
    public object? DefaultValue => _defaultValue.Value;

    /// <summary>
    /// Writes the value of a shadow property to the shared broker.
    /// </summary>
    /// <param name="entity">The entity that owns the shadow property.</param>
    /// <param name="value">The value of the shadow property.</param>
    public void WriteProperty(object entity, object? value) =>
        Broker.WritePropertyValue(entity, PropertyName, value);

    /// <summary>
    /// Reads the value of the shadow property from the shared broker for the specified entity.
    /// </summary>
    /// <param name="entity">The entity to read the shadow property value from.</param>
    /// <returns>The value of the shadow property.</returns>
    public object? ReadProperty(object entity) =>
        Broker.ReadPropertyValue(entity, PropertyName);
}
