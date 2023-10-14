using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Cosmodust.Cosmos.Tests;

public class CosmosTextFixture
{
    public IConfiguration Configuration { get; }
    public CosmosClient Client { get; }
    public CosmosDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public CosmosTextFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddUserSecrets<CosmosTextFixture>()  // local development user secrets
            .AddEnvironmentVariables()     // CI/CD
            .Build();

        EntityConfiguration = new EntityConfigurationHolder();

        Client = new CosmosClient(Configuration["ConnectionStrings__CosmosDB"], new CosmosClientOptions()
        {
            Serializer = new CosmosJsonSerializer(new IJsonTypeModifier[] { new BackingFieldJsonTypeModifier(EntityConfiguration) })
        });

        var db = Client.GetDatabase("reminderdb");
        Database = new CosmosDatabase(db);
    }
}
