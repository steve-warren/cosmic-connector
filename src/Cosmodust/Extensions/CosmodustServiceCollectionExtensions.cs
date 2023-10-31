using Cosmodust.Cosmos;
using Cosmodust.Json;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Store;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CosmodustServiceCollectionExtensions
{
    /// <summary>
    /// Adds the necessary services for using Cosmodust with Cosmos DB to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="cosmodustOptionsAction">An <see cref="Action{T}"/> to configure the <see cref="CosmodustOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddCosmodust(
        this IServiceCollection services,
        Action<CosmodustOptions> cosmodustOptionsAction)
    {
        services.Configure(cosmodustOptionsAction)
            .AddSingleton<ShadowPropertyStore>()
            .AddSingleton<EntityConfigurationProvider>()
            .AddSingleton<SqlParameterCache>()
            .AddSingleton<CosmodustJsonSerializer>(sp =>
            {
                var entityConfigurationHolder = sp.GetRequiredService<EntityConfigurationProvider>();

                return new CosmodustJsonSerializer(
                    new IJsonTypeModifier[]
                    {
                        new BackingFieldJsonTypeModifier(entityConfigurationHolder),
                        new PropertyJsonTypeModifier(entityConfigurationHolder),
                        new PartitionKeyJsonTypeModifier(entityConfigurationHolder),
                        new ShadowPropertyJsonTypeModifier(entityConfigurationHolder),
                        new TypeMetadataJsonTypeModifier()
                    });
            })
            .AddSingleton<CosmosClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<CosmodustOptions>>().Value;
                var client = new CosmosClient(
                    options.ConnectionString,
                    new CosmosClientOptions
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        Serializer = sp.GetRequiredService<CosmodustJsonSerializer>()
                    });

                return client;
            })
            .AddScoped<DocumentSession>(sp =>
            {
                var store = sp.GetRequiredService<DocumentStore>();
                return store.CreateSession();
            })
            .AddSingleton<QueryFacade>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<CosmodustOptions>>().Value;

                return new QueryFacade(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName: options.DatabaseId,
                    sp.GetRequiredService<SqlParameterCache>());
            })
            .AddSingleton<DocumentStore>(sp =>
            {
                var cosmodustOptions = sp.GetRequiredService<IOptions<CosmodustOptions>>().Value;

                var client = sp.GetRequiredService<CosmosClient>();
                var database = new CosmosDatabase(client.GetDatabase(id: cosmodustOptions.DatabaseId));
                var jsonSerializerOptions = sp.GetRequiredService<CosmodustJsonSerializer>().Options;
                
                var store = new DocumentStore(
                    database,
                    jsonSerializerOptions,
                    sp.GetRequiredService<EntityConfigurationProvider>(),
                    sqlParameterCache: sp.GetRequiredService<SqlParameterCache>(),
                    shadowPropertyStore: sp.GetRequiredService<ShadowPropertyStore>());

                store.BuildModel(cosmodustOptions.ModelBuilder);
                
                return store;
            });
        
        return services;
    }
}
