namespace CosmicConnector.Tests;

public sealed class MockDatabaseFacade : IDatabaseFacade
{
    private readonly Dictionary<(Type EntityType, string Id), object> _entities = new();

    public int Count => _entities.Count;

    public ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        if (_entities.TryGetValue((typeof(TEntity), id), out var entity))
            return new ValueTask<TEntity?>((TEntity?) entity);

        return default;
    }

    public void Add(string id, object entity)
    {
        _entities.Add((entity.GetType(), id), entity);
    }
}
