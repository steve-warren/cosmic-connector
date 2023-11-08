using Cosmodust.Shared;

namespace Cosmodust.Store;

public readonly record struct PolymorphicDerivedType
{
    public PolymorphicDerivedType(Type interfaceType,
        Type derivedType,
        string? typeDiscriminator = null)
    {
        Ensure.NotNull(interfaceType);
        Ensure.NotNull(derivedType);

        if (!interfaceType.IsInterface)
            throw new ArgumentException($"'{interfaceType.Name}' is not an interface.");

        if (!interfaceType.IsAssignableFrom(derivedType))
            throw new ArgumentException(
                $"Type '{derivedType.Name}' does not implement interface '{interfaceType.Name}'.");
        
        InterfaceType = interfaceType;
        DerivedType = derivedType;
        TypeDiscriminator = string.IsNullOrEmpty(typeDiscriminator)
            ? derivedType.Name
            : typeDiscriminator;
    }

    public Type InterfaceType { get; }
    public Type DerivedType { get; }
    public string TypeDiscriminator { get; }
}
