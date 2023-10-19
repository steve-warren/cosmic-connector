using System.Linq.Expressions;
using Cosmodust.Store;
using Cosmodust.Tracking;

namespace Cosmodust.Linq;

internal sealed class CosmodustLinqQueryProvider : IQueryProvider
{
    private readonly IQueryProvider _cosmosLinqQueryProvider;

    public CosmodustLinqQueryProvider(
        IDatabase database,
        EntityConfiguration entityConfiguration,
        ChangeTracker changeTracker,
        string? partitionKey,
        IQueryProvider cosmosLinqQueryProvider)
    {
        Database = database;
        EntityConfiguration = entityConfiguration;
        ChangeTracker = changeTracker;
        PartitionKey = partitionKey;
        _cosmosLinqQueryProvider = cosmosLinqQueryProvider;
    }

    public IDatabase Database { get; }
    public ChangeTracker ChangeTracker { get; }
    public EntityConfiguration EntityConfiguration { get; }
    public string? PartitionKey { get; }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        var cosmosLinqQuery = _cosmosLinqQueryProvider.CreateQuery<TElement>(expression);
        var query = new CosmodustLinqQuery<TElement>(cosmosLinqQuery, this);

        return query;
    }

    public IQueryable CreateQuery(Expression expression) =>
        throw new NotSupportedException("Synchronous queries are not supported.");

    public object? Execute(Expression expression) =>
        throw new NotSupportedException("Synchronous queries are not supported.");

    public TResult Execute<TResult>(Expression expression) =>
        throw new NotSupportedException("Synchronous queries are not supported.");
}
