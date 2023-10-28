using Cosmodust.Session;

namespace Cosmodust.Serialization;

/// <summary>
/// Represents a shadow property that can be used to store additional data for an entity.
/// </summary>
public class ShadowProperty
{
    public required string PropertyName { get; init; }
    public required Type PropertyType { get; init; }
    public required ShadowPropertyCache Cache { get; init; }

    public void SetValue(object entity, object? value)
    {
        if (value is null) return;

        Cache.SetShadowPropertyValue(entity, PropertyName, value);
    }
}
