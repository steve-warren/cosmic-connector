using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Store;

namespace Cosmodust.Json;

public class PolymorphicDerivedTypeModifier : IJsonTypeModifier
{
    private readonly Dictionary<Type,HashSet<PolymorphicDerivedType>> _polymorphicDerivedTypes = new();

    public void AddPolymorphicDerivedType(PolymorphicDerivedType polymorphicDerivedType)
    {
        if (!_polymorphicDerivedTypes.TryGetValue(polymorphicDerivedType.InterfaceType, out var derivedTypes))
            _polymorphicDerivedTypes.Add(polymorphicDerivedType.InterfaceType,
                new HashSet<PolymorphicDerivedType>() { polymorphicDerivedType });

        else
            derivedTypes.Add(polymorphicDerivedType);
    }
    
    public void Modify(JsonTypeInfo jsonTypeInfo)
    {
        if (!_polymorphicDerivedTypes.TryGetValue(jsonTypeInfo.Type, out var derivedTypes))
            return;
    
        var polymorphismOptions = new JsonPolymorphismOptions
        {
            IgnoreUnrecognizedTypeDiscriminators = false,
            TypeDiscriminatorPropertyName = "_type",
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };
        
        foreach(var derivedType in derivedTypes)
            polymorphismOptions.DerivedTypes.Add(
                new JsonDerivedType(
                    derivedType.DerivedType,
                    derivedType.TypeDiscriminator));

        jsonTypeInfo.PolymorphismOptions = polymorphismOptions;
    }
}
