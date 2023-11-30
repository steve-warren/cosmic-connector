namespace Cosmodust.Store;

public class StringSetter<TEntity> : IStringSetter
{
    private readonly Action<TEntity, string?> _func;

    public StringSetter(Action<TEntity, string?> func)
    {
        _func = func;
    }

    public void SetString(object entity, object? value)
    {
        if (value is not string str)
            throw new ArgumentException("value must be of type string.");

        if (entity is not TEntity typedEntity)
            throw new ArgumentException($"Entity must be of type {typeof(TEntity).Name}.", nameof(entity));

        _func(typedEntity, str);
    }
}
