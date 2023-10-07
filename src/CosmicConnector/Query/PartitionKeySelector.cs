namespace CosmicConnector.Query;

public class PartitionKeySelector<TEntity> : IPartitionKeySelector
{
    private readonly Func<TEntity, string> _partitionKeySelector;

    public PartitionKeySelector(Func<TEntity, string> partitionKeySelector)
    {
        _partitionKeySelector = partitionKeySelector;
    }

    public string GetPartitionKey(object entity)
    {
        if (entity is not TEntity typedEntity)
            throw new ArgumentException($"Entity must be of type {typeof(TEntity).Name}", nameof(entity));

        return _partitionKeySelector(typedEntity);
    }
}
