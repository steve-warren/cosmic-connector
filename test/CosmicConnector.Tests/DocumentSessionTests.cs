namespace CosmicConnector.Tests;

public class DocumentStoreTests
{
    private record ReminderList(string Id);
    private record Reminder(string Id);

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

        Action action = () => session.Store(null as ReminderList);

        action.Should().Throw<ArgumentNullException>(because: "we should not be able to store a null entity");
    }

    [Fact]
    public async Task Finding_Entity_By_Id_Should_Return_Same_Instance()
    {
        var documentStore = new DocumentStore();
        documentStore.ConfigureEntity<ReminderList>();

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);

        var storedEntity = await session.FindAsync<ReminderList>("id");

        storedEntity.Should().BeSameAs(entity, because: "we should get the same entity instance back");
    }

    [Fact]
    public async Task Entity_Not_Found_Should_Return_Null()
    {
        var documentStore = new DocumentStore();
        documentStore.ConfigureEntity<ReminderList>();

        var session = documentStore.CreateSession();

        var storedEntity = await session.FindAsync<ReminderList>("id");

        storedEntity.Should().BeNull(because: "we should not find an entity with the specified ID");
    }

    [Fact]
    public async Task Can_Store_Entities_Of_Different_Types()
    {
        var documentStore = new DocumentStore()
                            .ConfigureEntity<ReminderList>()
                            .ConfigureEntity<Reminder>();

        var session = documentStore.CreateSession();

        var entity1 = new ReminderList("id1");
        var entity2 = new Reminder("id2");

        session.Store(entity1);
        session.Store(entity2);

        var storedEntity1 = await session.FindAsync<ReminderList>("id1");
        var storedEntity2 = await session.FindAsync<Reminder>("id2");

        storedEntity1.Should().BeSameAs(entity1, because: "we should get the same entity instance back");
        storedEntity2.Should().BeSameAs(entity2, because: "we should get the same entity instance back");
    }

    [Fact]
    public void Given_Unregistered_Entity_Type_Then_Store_Should_Throw()
    {
        var documentStore = new DocumentStore();

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        Action action = () => session.Store(entity);

        action.Should().Throw<InvalidOperationException>(because: "we should not be able to store an entity that has not been configured");
    }

    [Fact]
    public async Task Given_Unregistered_Entity_Type_Then_Find_Should_Throw()
    {
        var documentStore = new DocumentStore();

        var session = documentStore.CreateSession();

        Func<Task> action = async () => await session.FindAsync<ReminderList>("id");

        await action.Should().ThrowAsync<InvalidOperationException>(because: "we should not be able to find an entity that has not been configured");
    }
}
