using System.Linq.Expressions;

namespace Cosmodust.Serialization;

public record ValueObjectConverter(
    Type ValueObjectType,
    Func<object, object?> ToValueObject,
    Func<object, object?> FromValueObject)
{
    public static ValueObjectConverter Create(Type valueObjectType)
    {
        var toValueObject = CreateParseLambda(valueObjectType);
        var fromValueObject = CreateToStringLambda(valueObjectType);

        return new ValueObjectConverter(valueObjectType, toValueObject, fromValueObject);
    }

    private static Func<object, object?> CreateToStringLambda(Type valueObjectType)
    {
        var instanceParameter = Expression.Parameter(valueObjectType, "instance");
        var toStringMethod = valueObjectType.GetMethod("ToString", Array.Empty<Type>())!;

        var callExpression = Expression.Call(instanceParameter, toStringMethod);
        var lambda = Expression.Lambda<Func<object, object?>>(callExpression, instanceParameter);

        return lambda.Compile();
    }

    private static Func<object, object?> CreateParseLambda(Type valueObjectType)
    {
        var nameParameter = Expression.Parameter(typeof(string), "name");
        var parseMethod = valueObjectType.GetMethod("Parse", new[] { typeof(string) });

        if (parseMethod is null)
            throw new InvalidOperationException(
                "A ValueObject must have a static Parse method accepting a single string argument.");

        // instance is null because Parse should be a static method
        var callExpression = Expression.Call(instance: null, parseMethod, nameParameter);
        var lambda = Expression.Lambda<Func<object, object?>>(callExpression, nameParameter);

        return lambda.Compile();
    }
}
