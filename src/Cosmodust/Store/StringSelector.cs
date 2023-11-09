namespace Cosmodust.Store;

internal sealed class StringSelector<TEntity> : IStringSelector
{
    private readonly Func<TEntity, string> _func;

    public StringSelector(Func<TEntity, string> func)
    {
        _func = func;
    }

    public string GetString(object entity)
    {
        if (entity is not TEntity typedEntity)
            throw new ArgumentException($"Entity must be of type {typeof(TEntity).Name}", nameof(entity));

        return _func(typedEntity);
    }
}
