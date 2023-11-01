namespace Cosmodust.Tracking;

/// <summary>
/// Represents a shadow property that can be used to store additional data for an entity.
/// </summary>
public class ShadowProperty
{
    private readonly Lazy<object?> _defaultValue;

    public ShadowProperty() =>
        _defaultValue = new Lazy<object?>(valueFactory: () =>
            PropertyType == typeof(string)
                ? default
                : Activator.CreateInstance(PropertyType ?? throw new InvalidOperationException()));

    public required string PropertyName { get; init; }
    public required Type PropertyType { get; init; }
    public required JsonSerializerPropertyStore Store { get; init; }
    public object? DefaultValue => _defaultValue.Value;

    /// <summary>
    /// Writes the value of a shadow property to the shared store.
    /// </summary>
    /// <param name="entity">The entity that owns the shadow property.</param>
    /// <param name="value">The value of the shadow property.</param>
    public void WritePropertyToSharedStore(object entity, object? value) =>
        Store.WritePropertyValue(entity, PropertyName, value);

    /// <summary>
    /// Reads the value of the shadow property from the shared store for the specified entity.
    /// </summary>
    /// <param name="entity">The entity to read the shadow property value from.</param>
    /// <returns>The value of the shadow property.</returns>
    public object? ReadPropertyFromSharedStore(object entity) =>
        Store.ReadPropertyValue(entity, PropertyName);
}
