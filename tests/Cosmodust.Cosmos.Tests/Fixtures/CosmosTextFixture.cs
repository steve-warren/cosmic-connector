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

    public CosmosTextFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddUserSecrets<CosmosTextFixture>()  // local development user secrets
            .AddEnvironmentVariables()     // CI/CD
            .Build();
    }
}
