using CosmoDust.Query;

namespace CosmoDust;

public class EntityBuilder<TEntity> : IEntityBuilder where TEntity : class
{
    private readonly EntityConfiguration _entityConfiguration;

    public EntityBuilder()
    {
        _entityConfiguration = new EntityConfiguration(typeof(TEntity));
    }

    public EntityBuilder<TEntity> HasId(Func<TEntity, string> idSelector)
    {
        _entityConfiguration.IdSelector = new StringSelector<TEntity>(idSelector);

        return this;
    }

    public EntityBuilder<TEntity> HasPartitionKey(Func<TEntity, string> partitionKeySelector)
    {
        _entityConfiguration.PartitionKeySelector = new StringSelector<TEntity>(partitionKeySelector);

        return this;
    }

    public EntityBuilder<TEntity> HasField(string name)
    {
        return this;
    }

    public void ToContainer(string containerName)
    {
        _entityConfiguration.ContainerName = containerName;
    }

    EntityConfiguration IEntityBuilder.Build() => _entityConfiguration;
}
