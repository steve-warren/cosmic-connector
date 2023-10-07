namespace CosmicConnector.Linq;

public static class DocumentQueryExtensions
{
    public static async Task<List<TEntity>> ToListAsync<TEntity>(this IQueryable<TEntity> queryable, CancellationToken cancellationToken = default) where TEntity : class
    {
        if (queryable is not DocumentQuery<TEntity> documentQueryable)
            throw new ArgumentException($"The {nameof(queryable)} must be of type {nameof(DocumentQuery<TEntity>)}", nameof(queryable));

        var query = documentQueryable.DocumentSession.DatabaseFacade.ExecuteQuery(documentQueryable.OriginalQueryable);

        var list = new List<TEntity>();

        await foreach (var entity in query)
        {
            documentQueryable.DocumentSession.IdentityMap.Attach(entity);
            documentQueryable.DocumentSession.ChangeTracker.RegisterUnchanged(entity);

            list.Add(entity);
        }

        return list;
    }
}
