using System.Linq.Expressions;
using System.Reflection;

namespace Cosmodust.Serialization;

public record PropertyAccessor(
    string PropertyName,
    Type PropertyType,
    Func<object, object?> Getter,
    Action<object, object?> Setter)
{
    public static PropertyAccessor Create(string propertyName, Type type)
    {
        var propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"The type '{type.Name}' does not have a property named '{propertyName}'.");

        return Create(propertyInfo);
    }

    public static PropertyAccessor Create<TEntity>(Expression<Func<TEntity, object>> expression)
    {
        var memberExpression = expression.Body as MemberExpression ?? throw new ArgumentException("The expression must be a member expression.", nameof(expression));
        var propertyInfo = memberExpression.Member as PropertyInfo ?? throw new ArgumentException("The expression must be a property expression.", nameof(expression));

        return Create(propertyInfo);
    }

    private static PropertyAccessor Create(PropertyInfo propertyInfo)
    {
        if ((propertyInfo.CanWrite & propertyInfo.CanRead) == false)
            throw new InvalidOperationException("The property must have a getter and a setter.");

        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var instanceCast = Expression.Convert(instance, propertyInfo.DeclaringType!);
        var valueCast = Expression.Convert(value, propertyInfo.PropertyType);

        var getter = Expression.Lambda<Func<object, object?>>(Expression.Convert(Expression.Property(instanceCast, propertyInfo), typeof(object)), instance).Compile();
        var setter = Expression.Lambda<Action<object, object?>>(Expression.Assign(Expression.Property(instanceCast, propertyInfo), valueCast), instance, value).Compile();

        return new PropertyAccessor(propertyInfo.Name, propertyInfo.PropertyType, getter, setter);
    }
}
