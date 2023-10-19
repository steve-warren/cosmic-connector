namespace Cosmodust.Store;

/// <summary>
/// A class that allows building entity configurations for a data model.
/// </summary>
public class ModelBuilder
{
    private readonly List<IEntityBuilder> _entityBuilders = new();

    /// <summary>
    /// Returns an instance of <see cref="EntityBuilder{TEntity}"/> for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be built.</typeparam>
    /// <returns>An instance of <see cref="EntityBuilder{TEntity}"/>.</returns>
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
