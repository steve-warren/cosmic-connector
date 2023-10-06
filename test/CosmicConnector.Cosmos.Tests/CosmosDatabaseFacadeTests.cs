using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CosmicConnector.Cosmos.Tests;

public class CosmosDatabaseFacadeTests : IClassFixture<CosmosTextFixture>
{
    public record AccountPlan(string Id);

    private readonly CosmosDatabaseFacade _db;

    public CosmosDatabaseFacadeTests(CosmosTextFixture configurationTextFixture)
    {
        _db = new CosmosDatabaseFacade(configurationTextFixture.Client);
    }

    [Fact]
    public async Task Can_Add_And_Find_Entity_In_Separate_Sessions()
    {
        var store = new DocumentStore(_db)
            .ConfigureEntity<AccountPlan>("reminderdb", "accountPlans");

        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = store.CreateSession();

        writeSession.Store(entity);
        await writeSession.SaveChangesAsync();

        var readSession = store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");
    }

    [Fact]
    public async Task Can_Add_Find_And_Delete_Entity()
    {
        var store = new DocumentStore(_db)
            .ConfigureEntity<AccountPlan>("reminderdb", "accountPlans");

        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = store.CreateSession();

        writeSession.Store(entity);
        await writeSession.SaveChangesAsync();

        var deleteSession = store.CreateSession();
        var deleteEntity = await deleteSession.FindAsync<AccountPlan>(entity.Id);

        deleteEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");
        deleteEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");

        deleteSession.Remove(deleteEntity!);
        await deleteSession.SaveChangesAsync();

        var readSession = store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().BeNull(because: "we should not be able to find the entity we just deleted");
    }
}
