namespace Cosmodust.Query;

public static class SqlQueryExtensions
{
    public static async Task<List<TEntity>> ToListAsync<TEntity>(
        this SqlQuery<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        var list = new List<TEntity>();

        await foreach(var item in query.ToAsyncEnumerable(cancellationToken))
            list.Add(item);

        return list;
    }

    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(
        this SqlQuery<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        await foreach (var item in query.ToAsyncEnumerable(cancellationToken))
            return item;

        return default;
    }
}
