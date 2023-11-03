using Cosmodust.Shared;
using Cosmodust.Store;

namespace Cosmodust.Extensions;

/// <summary>
/// Options for configuring the Cosmodust client.
/// </summary>
public class CosmodustOptions
{
    internal string ConnectionString { get; private set; } = "";
    internal string DatabaseId { get; private set; } = "";
    internal Action<ModelBuilder> ModelBuilder { get; private set; } = _ => { };
    internal CosmodustQueryOptions? QueryOptions { get; private set; }

    /// <summary>
    /// Sets the connection string to be used by the Cosmodust client.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    /// <returns>The updated <see cref="CosmodustOptions"/> instance.</returns>
    public CosmodustOptions WithConnectionString(string connectionString)
    {
        Ensure.NotNullOrWhiteSpace(connectionString);

        ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Sets the ID of the Cosmos DB database to use.
    /// </summary>
    /// <param name="databaseId">The ID of the database.</param>
    /// <returns>The updated <see cref="CosmodustOptions"/> instance.</returns>
    public CosmodustOptions WithDatabase(string databaseId)
    {
        Ensure.NotNullOrWhiteSpace(databaseId);

        DatabaseId = databaseId;
        return this;
    }

    /// <summary>
    /// Sets the model for the CosmodustOptions using the provided <paramref name="modelBuilder"/>.
    /// </summary>
    /// <param name="modelBuilder">The action to configure the model.</param>
    /// <returns>The updated CosmodustOptions.</returns>
    public CosmodustOptions WithModel(Action<ModelBuilder> modelBuilder)
    {
        Ensure.NotNull(modelBuilder);

        ModelBuilder = modelBuilder;
        return this;
    }

    public CosmodustOptions WithJsonOptions(
        Action<CosmodustJsonOptions> jsonOptions)
    {
        Ensure.NotNull(jsonOptions);
        return this;
    }

    public CosmodustOptions WithQueryOptions(
        Action<CosmodustQueryOptions> queryOptions)
    {
        Ensure.NotNull(queryOptions);

        QueryOptions = new CosmodustQueryOptions();

        queryOptions(QueryOptions);

        return this;
    }
}
