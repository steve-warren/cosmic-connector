using Cosmodust.Shared;

namespace Cosmodust.Store;

public readonly record struct PolymorphicDerivedType
{
    public PolymorphicDerivedType(Type baseOrInterfaceType,
        Type derivedType,
        string? typeDiscriminator = null)
    {
        Ensure.NotNull(baseOrInterfaceType);
        Ensure.NotNull(derivedType);

        if (!baseOrInterfaceType.IsAssignableFrom(derivedType))
            throw new ArgumentException(
                $"Type '{derivedType.Name}' does not implement interface or derive from '{baseOrInterfaceType.Name}'.");
        
        BaseOrInterfaceType = baseOrInterfaceType;
        DerivedType = derivedType;
        TypeDiscriminator = string.IsNullOrEmpty(typeDiscriminator)
            ? derivedType.Name
            : typeDiscriminator;
    }

    public Type BaseOrInterfaceType { get; }
    public Type DerivedType { get; }
    public string TypeDiscriminator { get; }
}
