using Cosmodust.Store;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class CosmodustOptions
{
    internal string ConnectionString { get; private set; } = "";
    internal string DatabaseId { get; private set; } = "";
    internal Action<ModelBuilder> ModelBuilder { get; private set; } = _ => { }; 

    public CosmodustOptions WithConnectionString(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }

    public CosmodustOptions WithDatabase(string databaseId)
    {
        DatabaseId = databaseId;
        return this;
    }

    public CosmodustOptions WithModel(Action<ModelBuilder> modelBuilder)
    {
        ModelBuilder = modelBuilder;
        return this;
    }
}
