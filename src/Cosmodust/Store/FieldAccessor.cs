using System.Linq.Expressions;
using System.Reflection;

namespace Cosmodust;

public record FieldAccessor(string FieldName, Type FieldType, Func<object, object?> Getter, Action<object, object?> Setter)
{
    public static FieldAccessor Create(string fieldName, Type type)
    {
        var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"The type '{type.Name}' does not have a field named '{fieldName}'.");

        return Create(fieldInfo);
    }

    public static FieldAccessor Create(FieldInfo fieldInfo)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var instanceCast = fieldInfo.IsStatic ? null : Expression.Convert(instance, fieldInfo.DeclaringType!);
        var valueCast = Expression.Convert(value, fieldInfo.FieldType);

        var getter = Expression.Lambda<Func<object, object?>>(Expression.Convert(Expression.Field(instanceCast, fieldInfo), typeof(object)), instance).Compile();
        var setter = Expression.Lambda<Action<object, object?>>(Expression.Assign(Expression.Field(instanceCast, fieldInfo), valueCast), instance, value).Compile();

        return new FieldAccessor(fieldInfo.Name, fieldInfo.FieldType, getter, setter);
    }
}
