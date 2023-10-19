using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Cosmodust.Store;
using Cosmodust.Tracking;

namespace Cosmodust.Linq;

public sealed class CosmodustLinqQuery<TEntity> : IQueryable<TEntity>
{
    public CosmodustLinqQuery(
        IDatabase database,
        ChangeTracker changeTracker,
        EntityConfiguration entityConfiguration,
        string? partitionKey,
        IQueryable<TEntity> cosmosLinqQuery)
    {
        Database = database;
        ChangeTracker = changeTracker;
        EntityConfiguration = entityConfiguration;
        PartitionKey = partitionKey;
        CosmosLinqQuery = cosmosLinqQuery;
        ElementType = cosmosLinqQuery.ElementType;
        Expression = cosmosLinqQuery.Expression;
        Provider = new CosmodustLinqQueryProvider(database, entityConfiguration, changeTracker, partitionKey, cosmosLinqQuery.Provider);
    }

    internal CosmodustLinqQuery(
        IQueryable<TEntity> cosmosLinqQuery,
        CosmodustLinqQueryProvider cosmodustLinqQueryProvider)
    {
        CosmosLinqQuery = cosmosLinqQuery;
        Database = cosmodustLinqQueryProvider.Database;
        EntityConfiguration = cosmodustLinqQueryProvider.EntityConfiguration;
        ChangeTracker = cosmodustLinqQueryProvider.ChangeTracker;
        PartitionKey = cosmodustLinqQueryProvider.PartitionKey;
        ElementType = cosmosLinqQuery.ElementType;
        Expression = cosmosLinqQuery.Expression;
        Provider = cosmodustLinqQueryProvider;
    }
    
    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }
    public IDatabase Database { get; }
    public ChangeTracker ChangeTracker { get; }
    public IQueryable<TEntity> CosmosLinqQuery { get; }
    public EntityConfiguration EntityConfiguration { get; }
    public string? PartitionKey { get; }

    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = Database.ToAsyncEnumerable(this, cancellationToken);

        await foreach (var entity in query)
        {
            Debug.Assert(entity is not null, "The entity should not be null.");

            ChangeTracker.RegisterUnchanged(entity);

            yield return entity;
        }
    }

    public IEnumerator<TEntity> GetEnumerator() => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}
