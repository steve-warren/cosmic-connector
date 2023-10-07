using System.Linq.Expressions;

namespace CosmicConnector.Linq;

internal sealed class DocumentQueryProvider<TEntity> : IQueryProvider
{
    private readonly DocumentSession _session;
    private readonly IQueryProvider _originalProvider;

    public DocumentQueryProvider(DocumentSession session, IQueryProvider originalProvider)
    {
        _session = session;
        _originalProvider = originalProvider;
    }

    public IQueryable CreateQuery(Expression expression) => throw new NotSupportedException();

    public object? Execute(Expression expression) => throw new NotSupportedException();

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new DocumentQuery<TElement>(_session, _originalProvider.CreateQuery<TElement>(expression));

    public TResult Execute<TResult>(Expression expression) => _originalProvider.Execute<TResult>(expression);
}
