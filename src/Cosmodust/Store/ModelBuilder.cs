using System.Linq.Expressions;
using System.Text.Json;
using Cosmodust.Json;

namespace Cosmodust.Store;

/// <summary>
/// A class that allows building entity configurations for a data model.
/// </summary>
public class ModelBuilder
{
    private readonly JsonSerializerOptions _options;
    private readonly List<IEntityBuilder> _entityBuilders = new();

    public ModelBuilder(JsonSerializerOptions options)
    {
        _options = options;
    }
    
    /// <summary>
    /// Returns an instance of <see cref="EntityBuilder{TEntity}"/> for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be built.</typeparam>
    /// <returns>An instance of <see cref="EntityBuilder{TEntity}"/>.</returns>
    public EntityBuilder<TEntity> HasEntity<TEntity>() where TEntity : class
    {
        var entityBuilder = new EntityBuilder<TEntity>();

        _entityBuilders.Add(entityBuilder);

        return entityBuilder;
    }

    public ModelBuilder HasConversion<TEntity, TConversion>(
        Expression<Func<TEntity, TConversion>> fromEntity,
        Expression<Func<TConversion, TEntity>> toEntity)
    {
        throw new NotImplementedException();
    }

    public ModelBuilder HasValueObject<TEnumeration>() where TEnumeration : class
    {
        var jsonConverter = new ValueObjectJsonConverter<TEnumeration>();

        _options.Converters.Add(jsonConverter);

        return this;
    }

    public ModelBuilder HasEnum<TEnum, TConversion>(
        Expression<Func<TEnum, TConversion>> fromEnum,
        Expression<Func<TConversion, TEnum>> toEnum) where TEnum : Enum
    {
        throw new NotImplementedException();
    }

    internal IReadOnlyList<EntityConfiguration> Build()
    {
        return _entityBuilders
            .Select(entityBuilder => entityBuilder.Build())
            .ToList();
    }
}
