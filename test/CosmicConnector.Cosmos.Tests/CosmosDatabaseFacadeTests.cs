using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CosmicConnector.Cosmos.Tests;

public class CosmosDatabaseFacadeTests : IClassFixture<ConfigurationTextFixture>
{
    public record AccountPlan(string Id);

    private readonly IConfiguration _configuration;

    public CosmosDatabaseFacadeTests(ConfigurationTextFixture configurationTextFixture)
    {
        _configuration = configurationTextFixture.Configuration;
    }

    [Fact]
    public async void Test1()
    {
        var entityMaps = new EntityMappingCollection();

        var db = new CosmosDatabaseFacade(_configuration["CosmosConnectionString"], entityMaps);

        var store = new DocumentStore(db, entityMaps)
            .MapEntity<AccountPlan>("reminderdb", "accountPlans");

        var session = store.CreateSession();

        var entity = await session.FindAsync<AccountPlan>("ap_2PGeLl3JtcXeLL9vvkApZyIhfhc");

        entity.Should().NotBeNull(because: "we should be able to find an entity");
    }
}
