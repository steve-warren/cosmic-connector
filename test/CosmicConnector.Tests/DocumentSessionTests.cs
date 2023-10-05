namespace CosmicConnector.Tests;

public class DocumentStoreTests
{
    private record TestEntity(string Id);

    [Fact]
    public void Can_Create_Session()
    {
        var documentStore = new DocumentStore();

        var session = documentStore.CreateSession();

        session.Should().NotBeNull(because: "we should be able to create a session");
    }

    [Fact]
    public void Storing_Null_Entity_Throws()
    {
        var documentStore = new DocumentStore();

        var session = documentStore.CreateSession();

        Action action = () => session.Store(null as TestEntity);

        action.Should().Throw<ArgumentNullException>(because: "we should not be able to store a null entity");
    }

    [Fact]
    public async Task Finding_Entity_By_Id_Should_Return_Same_Instance()
    {
        var documentStore = new DocumentStore();

        var session = documentStore.CreateSession();

        var entity = new TestEntity("id");

        session.Store(entity);

        var storedEntity = await session.FindAsync<TestEntity>("id");

        storedEntity.Should().BeSameAs(entity, because: "we should get the same entity instance back");
    }

    [Fact]
    public async Task Entity_Not_Found_Should_Return_Null()
    {
        var documentStore = new DocumentStore();

        var session = documentStore.CreateSession();

        var storedEntity = await session.FindAsync<TestEntity>("id");

        storedEntity.Should().BeNull(because: "we should not find an entity with the specified ID");
    }
}
