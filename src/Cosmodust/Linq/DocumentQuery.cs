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
        string partitionKey,
        IQueryable<TEntity> databaseLinqQuery)
    {
        Database = database;
        ChangeTracker = changeTracker;
        EntityConfiguration = entityConfiguration;
        PartitionKey = partitionKey;
        DatabaseLinqQuery = databaseLinqQuery;
        ElementType = databaseLinqQuery.ElementType;
        Expression = databaseLinqQuery.Expression;
        Provider = new CosmodustLinqQueryProvider(database, entityConfiguration, changeTracker, partitionKey, databaseLinqQuery.Provider);
    }

    internal CosmodustLinqQuery(
        IQueryable<TEntity> databaseLinqQuery,
        CosmodustLinqQueryProvider cosmodustLinqQueryProvider)
    {
        DatabaseLinqQuery = databaseLinqQuery;
        Database = cosmodustLinqQueryProvider.Database;
        EntityConfiguration = cosmodustLinqQueryProvider.EntityConfiguration;
        ChangeTracker = cosmodustLinqQueryProvider.ChangeTracker;
        PartitionKey = cosmodustLinqQueryProvider.PartitionKey;
        ElementType = databaseLinqQuery.ElementType;
        Expression = databaseLinqQuery.Expression;
        Provider = cosmodustLinqQueryProvider;
    }

    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }
    public IDatabase Database { get; }
    public ChangeTracker ChangeTracker { get; }
    public IQueryable<TEntity> DatabaseLinqQuery { get; }
    public EntityConfiguration EntityConfiguration { get; }
    public string PartitionKey { get; }

    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = Database.ToAsyncEnumerable(this, cancellationToken);

        await foreach (var entity in query)
        {
            Debug.Assert(entity is not null, "The entity should not be null.");

            var trackedEntity = ChangeTracker.GetOrRegisterUnchanged(entity);

            yield return (TEntity) trackedEntity;
        }
    }

    public IEnumerator<TEntity> GetEnumerator() => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}
