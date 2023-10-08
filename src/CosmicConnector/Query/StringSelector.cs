namespace CosmicConnector.Query;

internal class StringSelector<TEntity> : IStringSelector
{
    private readonly Func<TEntity, string> _partitionKeySelector;

    public StringSelector(Func<TEntity, string> partitionKeySelector)
    {
        _partitionKeySelector = partitionKeySelector;
    }

    public string GetString(object entity)
    {
        if (entity is not TEntity typedEntity)
            throw new ArgumentException($"Entity must be of type {typeof(TEntity).Name}", nameof(entity));

        return _partitionKeySelector(typedEntity);
    }
}
