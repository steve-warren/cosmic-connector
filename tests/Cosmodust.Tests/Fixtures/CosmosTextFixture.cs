using Microsoft.Extensions.Configuration;

namespace Cosmodust.Tests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class CosmosTextFixture
{
    public IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddUserSecrets<CosmosTextFixture>()  // local development user secrets
        .AddEnvironmentVariables()     // CI/CD
        .Build();
}
