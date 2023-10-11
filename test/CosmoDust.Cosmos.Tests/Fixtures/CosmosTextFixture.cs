using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CosmoDust.Cosmos.Tests;

public class CosmosTextFixture
{
    public IConfiguration Configuration { get; }
    public CosmosClient Client { get; }

    public CosmosTextFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddUserSecrets<CosmosTextFixture>()  // local development user secrets
            .AddEnvironmentVariables()     // CI/CD
            .Build();

        Client = new CosmosClient(Configuration["CosmosConnectionString"], new CosmosClientOptions()
        {
            Serializer = new CosmosJsonSerializer(new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        });
    }
}
