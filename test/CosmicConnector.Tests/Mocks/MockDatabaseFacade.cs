using CosmicConnector.Linq;

namespace CosmicConnector.Tests;

public sealed class MockDatabaseFacade : IDatabaseFacade
{
    private readonly Dictionary<(Type EntityType, string Id), object> _entities = new();

    public int Count => _entities.Count;
    public bool SaveChangesWasCalled { get; private set; }
    public EntityConfigurationHolder EntityConfiguration { get; set; } = new();

    public ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        if (_entities.TryGetValue((typeof(TEntity), id), out var entity))
            return new ValueTask<TEntity?>((TEntity?) entity);

        return default;
    }

    public IQueryable<TEntity> GetLinqQuery<TEntity>() where TEntity : class => _entities.Values.OfType<TEntity>().AsQueryable();

    public async IAsyncEnumerable<TEntity> ExecuteQuery<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
    {
        foreach (var entity in queryable)
        {
            await Task.Yield();
            yield return entity;
        }
    }

    public void Add(string id, object entity)
    {
        _entities.Add((entity.GetType(), id), entity);
    }

    public Task CommitAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default)
    {
        SaveChangesWasCalled = true;

        return Task.CompletedTask;
    }
}
