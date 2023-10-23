using System.Runtime.CompilerServices;
using Cosmodust.Linq;
using Cosmodust.Query;
using Cosmodust.Tracking;

namespace Cosmodust.Tests;

public sealed class MockDatabase : IDatabase
{
    private readonly Dictionary<(Type EntityType, string Id), object> _entities = new();

    public string Name { get; private set; } = "MockDatabase";
    public int Count => _entities.Count;
    public bool CommitWasCalled { get; private set; }

    public ValueTask<TEntity?> FindAsync<TEntity>(string containerName, string id, string partitionKey, CancellationToken cancellationToken = default)
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
        CommitWasCalled = true;

        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(IEnumerable<EntityEntry> entries, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> CreateLinqQuery<TEntity>(string containerName)
    {
        return _entities.Values.OfType<TEntity>().AsQueryable();
    }

    public IQueryable<TEntity> GetLinqQuery<TEntity>(string containerName, string? partitionKey = null)
    {
        return _entities.Values.OfType<TEntity>().AsQueryable();
    }

    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(CosmodustLinqQuery<TEntity> query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var entity in query.DatabaseLinqQuery)
        {
            await Task.Yield();
            yield return entity;
        }
    }

    public IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(SqlQuery<TEntity> query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
