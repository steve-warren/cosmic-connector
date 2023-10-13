namespace CosmoDust;

public class ModelBuilder
{
    private readonly List<IEntityBuilder> _entityBuilders = new();

    public EntityBuilder<TEntity> Entity<TEntity>() where TEntity : class
    {
        var entityBuilder = new EntityBuilder<TEntity>();

        _entityBuilders.Add(entityBuilder);

        return entityBuilder;
    }

    internal IReadOnlyList<EntityConfiguration> Build()
    {
        var configurations = new List<EntityConfiguration>();

        foreach (var entityBuilder in _entityBuilders)
            configurations.Add(entityBuilder.Build());

        return configurations;
    }
}
