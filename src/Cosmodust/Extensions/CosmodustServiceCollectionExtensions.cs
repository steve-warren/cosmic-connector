using System.Text.Json;
using Cosmodust.Json;
using Cosmodust.Memory;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Shared;
using Cosmodust.Store;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cosmodust.Extensions;

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
        Ensure.NotNull(cosmodustOptionsAction);

        services.AddSingleton(provider =>
        {
            var cosmodustJsonOptions = provider.GetRequiredService<CosmodustJsonOptions>();
            var builder = provider.GetRequiredService<ModelBuilder>();
            var options = new CosmodustOptions(cosmodustJsonOptions, builder);

            cosmodustOptionsAction.Invoke(options);
            return options;
        });
        services.AddSingleton<IMemoryStreamProvider, RecyclableMemoryStreamProvider>();
        services.AddSingleton<JsonPropertyBroker>();
        services.AddSingleton<EntityConfigurationProvider>();
        services.AddSingleton<SqlParameterObjectTypeResolver>();
        services.AddSingleton<CosmodustJsonOptions>();
        services.AddSingleton<CosmodustJsonSerializer>();
        services.AddSingleton<ModelBuilder>();

        services.AddSingleton<CosmosClient>(sp =>
            {
                var options = sp.GetRequiredService<CosmodustOptions>();
                var serializer = sp.GetRequiredService<CosmodustJsonSerializer>();

                var client = new CosmosClient(
                    options.ConnectionString,
                    new CosmosClientOptions { ConnectionMode = ConnectionMode.Direct, Serializer = serializer });

                return client;
            });
        services.AddScoped<DocumentSession>(sp =>
            {
                var store = sp.GetRequiredService<DocumentStore>();
                return store.CreateSession();
            });
        services.AddSingleton<QueryFacade>();
        services.AddSingleton<DocumentStore>();
        services.AddSingleton<IDatabase, CosmosDatabase>();

        return services;
    }
}
