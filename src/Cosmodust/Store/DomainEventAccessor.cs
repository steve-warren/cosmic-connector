using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Cosmodust.Store;

public readonly record struct DomainEventAccessor(
    string DomainEventCollectionFieldName,
    Func<string> IdFactoryDelegate,
    Func<object, IEnumerable<object>> IteratorDelegate,
    Action<object> ClearDelegate)
{
    public static readonly DomainEventAccessor Null = new DomainEventAccessor(
        "",
        () => string.Empty,
        _ => Enumerable.Empty<object>(),
        _ => { });

    public static DomainEventAccessor Create<TEntity>(
        string domainEventCollectionFieldName,
        Func<string> idFactoryDelegate)
    {
        var entityType = typeof(TEntity);

        var fieldInfo = entityType.GetField(
            domainEventCollectionFieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo == null)
            throw new InvalidOperationException(
                $"Field '{domainEventCollectionFieldName}' not found on type '{entityType}'.");

        var iterator = CompileIterationLambda(
            entityType,
            fieldInfo);

        var clearer = CompileClearLambda(
            entityType,
            fieldInfo);

        return new DomainEventAccessor(
            domainEventCollectionFieldName,
            idFactoryDelegate,
            iterator,
            clearer);
    }

    public string NextId() =>
        IdFactoryDelegate();

    public IEnumerable<object> GetDomainEvents(object entity) =>
        IteratorDelegate(entity);

    public void ClearDomainEvents(object entity) =>
        ClearDelegate(entity);

    private static Func<object, IEnumerable<object>> CompileIterationLambda(
        Type entityType,
        FieldInfo fieldInfo)
    {
        var target = Expression.Parameter(typeof(object), "entity");
        var castTarget = Expression.Convert(target, entityType);
        var fieldAccess = Expression.Field(castTarget, fieldInfo);
        var castFieldAccess = Expression.Convert(fieldAccess, typeof(IEnumerable));

        var lambda = Expression.Lambda<Func<object, IEnumerable<object>>>(
            Expression.Call(typeof(Enumerable), "Cast", new[] { typeof(object) }, castFieldAccess),
            target
        );

        return lambda.Compile();
    }

    private static Action<object> CompileClearLambda(
        Type entityType,
        FieldInfo fieldInfo)
    {
        var target = Expression.Parameter(typeof(object), "entity");
        var castTarget = Expression.Convert(target, entityType);
        var fieldAccess = Expression.Field(castTarget, fieldInfo);
        var clearMethod = fieldInfo.FieldType.GetMethod("Clear");

        if (clearMethod is null)
            throw new InvalidOperationException("Domain event collection must implement ICollection<T>.");

        var clearCall = Expression.Call(fieldAccess, clearMethod);
        var lambda = Expression.Lambda<Action<object>>(clearCall, target);

        return lambda.Compile();
    }
}
