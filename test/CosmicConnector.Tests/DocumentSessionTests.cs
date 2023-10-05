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

    [Fact]
    public void Stored_Entity_Should_Be_In_Added_State()
    {
        var documentStore = new DocumentStore()
                            .ConfigureEntity<ReminderList>();

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);

        session.ChangeTracker.Entries.Should().HaveCount(1, because: "we should have one entity in the change tracker");
        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Added, because: "we should have one entity in the change tracker in the added state");
    }

    [Fact]
    public async Task Call_To_SaveChanges_Should_Transition_Added_Entity_To_Unmodified_State()
    {
        var documentStore = new DocumentStore()
                            .ConfigureEntity<ReminderList>();

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);

        await session.SaveChangesAsync();

        session.ChangeTracker.Entries.Should().HaveCount(1, because: "we should have one entity in the change tracker");
        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Unchanged, because: "we should have one entity in the change tracker in the unchanged state");
    }

    [Fact]
    public void Updated_Entity_Should_Be_In_Modified_State()
    {
        var documentStore = new DocumentStore()
                            .ConfigureEntity<ReminderList>();

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);
        session.Update(entity);

        session.ChangeTracker.Entries.Should().HaveCount(1, because: "we should have one entity in the change tracker");
        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Modified, because: "we should have one entity in the change tracker in the modified state");
    }

    [Fact]
    public async Task Removed_Entity_Should_Be_In_Deleted_State_And_Removed_From_Tracking()
    {
        var documentStore = new DocumentStore()
                            .ConfigureEntity<ReminderList>();

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);
        session.Remove(entity);

        var trackedEntity = session.ChangeTracker.Entries[0];

        session.ChangeTracker.Entries.Should().HaveCount(1, because: "we should have one entity in the change tracker");
        trackedEntity.State.Should().Be(EntityState.Removed, because: "we should have one entity in the change tracker in the modified state");

        await session.SaveChangesAsync();

        session.ChangeTracker.Entries.Should().HaveCount(0, because: "we should have no entities in the change tracker");
        trackedEntity.State.Should().Be(EntityState.Removed, because: "we should have the entity in the removed state");
    }

    [Fact]
    public async Task Removed_Entity_Should_Be_Removed_From_IdentityMap()
    {
        var documentStore = new DocumentStore()
                            .ConfigureEntity<ReminderList>();

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);
        session.Remove(entity);

        await session.SaveChangesAsync();

        session.ChangeTracker.Entries.Should().HaveCount(0, because: "we should have no entities in the change tracker");
        session.IdentityMap.Exists<ReminderList>("id").Should().BeFalse(because: "we should not have the entity in the identity map");
    }
}
