using Microsoft.Extensions.Configuration;

namespace CosmicConnector.Cosmos.Tests;

public class ConfigurationTextFixture
{
    public IConfiguration Configuration { get; }

    public ConfigurationTextFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddUserSecrets<ConfigurationTextFixture>()  // local development user secrets
            .AddEnvironmentVariables()     // CI/CD
            .Build();
    }
}
