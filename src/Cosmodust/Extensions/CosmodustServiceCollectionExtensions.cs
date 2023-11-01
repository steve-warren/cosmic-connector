using Cosmodust;
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
            .AddSingleton<JsonSerializerPropertyStore>()
            .AddSingleton<EntityConfigurationProvider>()
            .AddSingleton<SqlParameterObjectTypeCache>()
            .AddSingleton<CosmodustJsonSerializer>(sp =>
            {
                var entityConfigurationProvider = sp.GetRequiredService<EntityConfigurationProvider>();
                var jsonSerializerPropertyStore = sp.GetRequiredService<JsonSerializerPropertyStore>();
                
                return new CosmodustJsonSerializer(
                    new IJsonTypeModifier[]
                    {
                        new BackingFieldJsonTypeModifier(entityConfigurationProvider),
                        new PropertyJsonTypeModifier(entityConfigurationProvider),
                        new PartitionKeyJsonTypeModifier(entityConfigurationProvider),
                        new ShadowPropertyJsonTypeModifier(entityConfigurationProvider),
                        new TypeMetadataJsonTypeModifier(),
                        new DocumentETagJsonTypeModifier(entityConfigurationProvider, jsonSerializerPropertyStore)
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
                    sp.GetRequiredService<SqlParameterObjectTypeCache>());
            })
            .AddSingleton<DocumentStore>(sp =>
            {
                var cosmodustOptions = sp.GetRequiredService<IOptions<CosmodustOptions>>().Value;

                var client = sp.GetRequiredService<CosmosClient>();
                var linqSerializerOptions = new CosmosLinqSerializerOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                };
                var database = new CosmosDatabase(client.GetDatabase(id: cosmodustOptions.DatabaseId),linqSerializerOptions);
                var jsonSerializerOptions = sp.GetRequiredService<CosmodustJsonSerializer>().Options;
                
                var store = new DocumentStore(
                    database,
                    jsonSerializerOptions,
                    sp.GetRequiredService<EntityConfigurationProvider>(),
                    sqlParameterCache: sp.GetRequiredService<SqlParameterObjectTypeCache>(),
                    shadowPropertyStore: sp.GetRequiredService<JsonSerializerPropertyStore>());

                store.DefineModel(cosmodustOptions.ModelBuilder);
                
                return store;
            });
        
        return services;
    }
}
