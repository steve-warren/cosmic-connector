using System.Runtime.CompilerServices;
using Cosmodust.Shared;
using Cosmodust.Store;
using Cosmodust.Tracking;

namespace Cosmodust.Query;

/// <summary>
/// Represents a SQL query that can be executed against a database
/// and returns a sequence of results of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the query result.</typeparam>
public class SqlQuery<TEntity>
{
    private readonly HashSet<(string Name, object? Value)> _parameters;

    public SqlQuery(
        IDatabase database,
        ChangeTracker changeTracker,
        EntityConfiguration entityConfiguration,
        string sql,
        string partitionKey,
        IEnumerable<(string Name, object? Value)> parameters)
    {
        Database = database;
        ChangeTracker = changeTracker;
        EntityConfiguration = entityConfiguration;
        Sql = sql;
        PartitionKey = partitionKey;

        _parameters = new HashSet<(string Name, object? Value)>(parameters);
    }

    private IDatabase Database { get; }
    private ChangeTracker ChangeTracker { get; }
    public EntityConfiguration EntityConfiguration { get; }
    public string Sql { get; }
    public string PartitionKey { get; }
    public Type Type => typeof(TEntity);
    public IEnumerable<(string Name, object? Value)> Parameters => _parameters;

    /// <summary>
    /// Adds a parameter to the query.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>The current <see cref="SqlQuery{TResult}"/> instance.</returns>
    public SqlQuery<TEntity> WithParameter(string name, object? value)
    {
        _parameters.Add((Name: name, Value: value));
        return this;
    }

    /// <summary>
    /// Converts the SQL query to an asynchronous enumerable of results.
    /// </summary>
    /// <typeparam name="TEntity">The type of the query result.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of query results.</returns>
    public async IAsyncEnumerable<TEntity> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var items = Database.ToAsyncEnumerable(this, cancellationToken);

        await foreach (var item in items)
        {
            Ensure.NotNull(item);
            ChangeTracker.RegisterUnchanged(item, "");
            yield return item;
        }
    }
}
