using System.Runtime.CompilerServices;

namespace CosmicConnector.Tests;

public sealed class MockDatabase : IDatabase
{
    private readonly Dictionary<(Type EntityType, string Id), object> _entities = new();

    public int Count => _entities.Count;
    public bool SaveChangesWasCalled { get; private set; }
    public EntityConfigurationHolder EntityConfiguration { get; set; } = new();

    public ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        if (_entities.TryGetValue((typeof(TEntity), id), out var entity))
            return new ValueTask<TEntity?>((TEntity?) entity);

        return default;
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

    public IQueryable<TEntity> GetLinqQuery<TEntity>(string? partitionKey = null)
    {
        return _entities.Values.OfType<TEntity>().AsQueryable();
    }

    public async IAsyncEnumerable<TEntity> GetAsyncEnumerable<TEntity>(IQueryable<TEntity> queryable, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var entity in queryable)
        {
            await Task.Yield();
            yield return entity;
        }
    }
}
