using Cosmodust.Cosmos;
using Cosmodust.Json;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Store;
using Microsoft.Azure.Cosmos;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CosmodustServiceCollectionExtensions
{
    public static IServiceCollection AddCosmodust(
        this IServiceCollection services,
        string connectionString,
        string database,
        Action<CosmodustOptionsBuilder>? cosmodustOptionsAction = null)
    {
        if (cosmodustOptionsAction != null)
            services.Configure(cosmodustOptionsAction);

        services.AddSingleton<ShadowPropertyStore>();
        services.AddSingleton<EntityConfigurationProvider>();
        services.AddSingleton<SqlParameterCache>();
        services.AddSingleton<CosmodustJsonSerializer>(sp =>
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
        });
        services.AddSingleton<CosmosClient>(sp =>
        {
            var client = new CosmosClient(
                connectionString,
                new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Direct,
                    Serializer = sp.GetRequiredService<CosmodustJsonSerializer>()
                });

            return client;
        });

        services.AddScoped<DocumentSession>(sp =>
        {
            var store = sp.GetRequiredService<DocumentStore>();
            return store.CreateSession();
        });

        services.AddSingleton<QueryFacade>(sp => new QueryFacade(
            sp.GetRequiredService<CosmosClient>(),
            databaseName: database,
            sp.GetRequiredService<SqlParameterCache>()));

        return services;
    }
}
