using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Cosmodust.Linq;

public sealed class CosmodustLinqQuery<TEntity> : IQueryable<TEntity>
{
    public CosmodustLinqQuery(IDatabase database, ChangeTracker changeTracker, IQueryable<TEntity> originalLinqQuery)
    {
        Database = database;
        ChangeTracker = changeTracker;
        OriginalLinqQuery = originalLinqQuery;
        ElementType = originalLinqQuery.ElementType;
        Expression = originalLinqQuery.Expression;
        Provider = new CosmodustLinqQueryProvider(database, changeTracker, originalLinqQuery.Provider);
    }

    internal CosmodustLinqQuery(IQueryable<TEntity> originalLinqQuery, CosmodustLinqQueryProvider cosmodustLinqQueryProvider)
    {
        OriginalLinqQuery = originalLinqQuery;
        Database = cosmodustLinqQueryProvider.Database;
        ChangeTracker = cosmodustLinqQueryProvider.ChangeTracker;
        ElementType = originalLinqQuery.ElementType;
        Expression = originalLinqQuery.Expression;
        Provider = cosmodustLinqQueryProvider;
    }

    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }
    internal IDatabase Database { get; }
    internal ChangeTracker ChangeTracker { get; }
    internal IQueryable<TEntity> OriginalLinqQuery { get; }

    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = Database.ToAsyncEnumerable(OriginalLinqQuery, cancellationToken);

        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            Debug.Assert(entity is not null, "The entity should not be null.");

            ChangeTracker.RegisterUnchanged(entity);

            yield return entity;
        }
    }

    public IEnumerator<TEntity> GetEnumerator() => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}
