namespace Cosmodust.Linq;

public static class DocumentQueryExtensions
{
    public static async Task<List<TEntity>> ToListAsync<TEntity>(this IQueryable<TEntity> queryable, CancellationToken cancellationToken = default)
    {
        var documentQuery = queryable.ToDocumentQuery();
        var iterator = documentQuery.ToAsyncEnumerable(cancellationToken);

        var list = new List<TEntity>();

        await foreach (var entity in iterator)
            list.Add(entity);

        return list;
    }

    public static IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(this IQueryable<TEntity> queryable, CancellationToken cancellationToken = default)
    {
        var documentQuery = queryable.ToDocumentQuery();
        return documentQuery.ToAsyncEnumerable(cancellationToken);
    }

    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(this IQueryable<TEntity> queryable, CancellationToken cancellationToken = default)
    {
        var documentQuery = queryable.Take(1).ToDocumentQuery();

        var iterator = documentQuery.ToAsyncEnumerable(cancellationToken);

        await foreach (var entity in iterator)
            return entity;

        return default;
    }

    private static CosmodustLinqQuery<TEntity> ToDocumentQuery<TEntity>(this IQueryable<TEntity> queryable)
    {
        if (queryable is not CosmodustLinqQuery<TEntity> documentQueryable)
            throw new ArgumentException($"The {nameof(queryable)} must be of type {nameof(CosmodustLinqQuery<TEntity>)}", nameof(queryable));

        return documentQueryable;
    }
}
