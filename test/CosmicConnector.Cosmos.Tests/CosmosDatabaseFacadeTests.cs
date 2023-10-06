using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CosmicConnector.Cosmos.Tests;

public class CosmosDatabaseFacadeTests : IClassFixture<CosmosTextFixture>
{
    public class AccountPlan
    {
        public AccountPlan(string id)
        {
            Id = id;
        }

        public string Id { get; init; }
        public string Name { get; set; } = "Test Plan";
    }

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
    public async Task Can_Add_Find_Delete_Entity_In_Separate_Sessions()
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

    [Fact]
    public async Task Can_Add_Find_Update_Entity_In_Separate_Session()
    {
        var store = new DocumentStore(_db)
            .ConfigureEntity<AccountPlan>("reminderdb", "accountPlans");

        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = store.CreateSession();

        writeSession.Store(entity);
        await writeSession.SaveChangesAsync();

        var updateSession = store.CreateSession();
        var updatedEntity = await updateSession.FindAsync<AccountPlan>(entity.Id);

        updatedEntity.Should().NotBeSameAs(entity, because: "the entity should be a different instance since it is from a different session");
        updatedEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");

        updatedEntity!.Name = "Updated Plan";
        updateSession.Update(updatedEntity);

        await updateSession.SaveChangesAsync();

        var readSession = store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().NotBeSameAs(updatedEntity, because: "the entity should be a different instance since it is from a different session");
        readEntity.Should().NotBeNull(because: "we should be able to find the entity we just updated");
        readEntity!.Name.Should().Be(updatedEntity.Name, because: "we should have updated the entity name");
    }
}
