using System.Collections;
using System.Linq.Expressions;

namespace CosmicConnector.Linq;

internal sealed class DocumentQuery<TEntity> : IQueryable<TEntity>
{
    public DocumentQuery(DocumentSession session, IQueryable<TEntity> queryable)
    {
        OriginalQueryable = queryable;
        DocumentSession = session;
        ElementType = queryable.ElementType;
        Expression = queryable.Expression;
        Provider = new DocumentQueryProvider<TEntity>(session, queryable.Provider);
    }

    public DocumentSession DocumentSession { get; }
    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }
    public IQueryable<TEntity> OriginalQueryable { get; }

    public IAsyncEnumerable<TEntity> GetAsyncEnumerable(CancellationToken cancellationToken = default) => DocumentSession.GetAsyncEnumerable(OriginalQueryable, cancellationToken);

    public IEnumerator<TEntity> GetEnumerator() => OriginalQueryable.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
