using Cosmodust.Linq;
using Cosmodust.Store;
using Cosmodust.Tracking;

namespace Cosmodust.Tests;

public class DocumentStoreTests
{
    private record ReminderList(string Id);
    private record Reminder(string Id);

    [Fact]
    public void Can_Create_Session()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database);

        var session = documentStore.CreateSession();

        session.Should().NotBeNull(because: "we should be able to create a session");
    }

    [Fact]
    public void Storing_Null_Entity_Throws()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        Action action = () => session.Store<ReminderList>(null!);

        action.Should().Throw<ArgumentNullException>(because: "we should not be able to store a null entity");
    }

    [Fact]
    public async Task Finding_Entity_By_Id_Should_Return_Same_Instance()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);

        var storedEntity = await session.FindAsync<ReminderList>("id");

        storedEntity.Should().BeSameAs(entity, because: "we should get the same entity instance back");
    }

    [Fact]
    public async Task Entity_Not_Found_Should_Return_Null()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        var storedEntity = await session.FindAsync<ReminderList>("id");

        storedEntity.Should().BeNull(because: "we should not find an entity with the specified ID");
    }

    [Fact]
    public async Task Can_Store_Entities_Of_Different_Types()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");

                builder.HasEntity<Reminder>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

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
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database);

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        Action action = () => session.Store(entity);

        action.Should().Throw<InvalidOperationException>(because: "we should not be able to store an entity that has not been configured");
    }

    [Fact]
    public async Task Given_Unregistered_Entity_Type_Then_Find_Should_Throw()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database);

        var session = documentStore.CreateSession();

        Func<Task> action = async () => await session.FindAsync<ReminderList>("id");

        await action.Should().ThrowAsync<InvalidOperationException>(because: "we should not be able to find an entity that has not been configured");
    }

    [Fact]
    public void Stored_Entity_Should_Be_In_Added_State()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);

        session.ChangeTracker.Entries.Should().HaveCount(1, because: "we should have one entity in the change tracker");
        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Added, because: "we should have one entity in the change tracker in the added state");
    }

    [Fact]
    public async Task Call_To_SaveChanges_Should_Transition_Added_Entity_To_Unmodified_State()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);

        await session.CommitAsync();

        session.ChangeTracker.Entries.Should().HaveCount(1, because: "we should have one entity in the change tracker");
        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Unchanged, because: "we should have one entity in the change tracker in the unchanged state");
    }

    [Fact]
    public void Updated_Entity_Should_Be_In_Modified_State()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

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
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);
        session.Remove(entity);

        var trackedEntity = session.ChangeTracker.Entries[0];

        session.ChangeTracker.Entries.Should().HaveCount(1, because: "we should have one entity in the change tracker");
        trackedEntity.State.Should().Be(EntityState.Removed, because: "we should have one entity in the change tracker in the modified state");

        await session.CommitAsync();

        session.ChangeTracker.Entries.Should().HaveCount(0, because: "we should have no entities in the change tracker");
        trackedEntity.State.Should().Be(EntityState.Detached, because: "we should have the entity in the detached state");
    }

    [Fact]
    public async Task Removed_Entity_Should_Be_Removed_From_IdentityMap()
    {
        var database = new MockDatabase();
        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        var entity = new ReminderList("id");

        session.Store(entity);
        session.Remove(entity);

        await session.CommitAsync();

        session.ChangeTracker.Entries.Should().HaveCount(0, because: "we should have no entities in the change tracker");
        session.ChangeTracker.Exists<ReminderList>("id").Should().BeFalse(because: "we should not have the entity in the identity map");
    }

    [Fact]
    public async Task Find_Entity_Should_Track_Entity_As_Unchanged()
    {
        var entity = new ReminderList("id");

        var database = new MockDatabase();
        database.Add("id", entity);

        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        _ = await session.FindAsync<ReminderList>("id");

        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Unchanged, because: "we should have one entity in the change tracker in the unchanged state after finding it");
    }

    [Fact]
    public async Task Call_To_SaveChanges_Should_Call_Database()
    {
        var entity = new ReminderList("id");

        var database = new MockDatabase();
        database.Add("id", entity);

        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        _ = await session.FindAsync<ReminderList>("id");

        await session.CommitAsync();

        database.CommitWasCalled.Should().BeTrue(because: "we should have called the database's CommitAsync method");
    }

    [Fact]
    public async Task Can_Query_Entity_By_Id_Using_Linq_Expression()
    {
        var entity = new ReminderList("id");

        var database = new MockDatabase();
        database.Add("id", entity);

        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        var result = await session.Query<ReminderList>()
                                  .Where(x => x.Id == "id")
                                  .ToListAsync();

        result.Should().HaveCount(1, because: "we should have one entity in the result set");
        result[0].Should().BeEquivalentTo(entity, because: "we should get the same entity instance back");
    }

    [Fact]
    public async void Query_Should_Attach_Entities_To_Change_Tracker()
    {
        var database = new MockDatabase();
        database.Add("id1", new ReminderList("id1"));
        database.Add("id2", new ReminderList("id2"));

        var documentStore = new DocumentStore(database)
            .BuildModel(builder =>
            {
                builder.HasEntity<ReminderList>()
                       .HasId(e => e.Id)
                       .ToContainer("db");
            });

        var session = documentStore.CreateSession();

        await session.Query<ReminderList>()
                    .Where(x => x.Id == "id1" || x.Id == "id2")
                    .ToListAsync();

        session.ChangeTracker.Entries.Should().HaveCount(2, because: "we should have two entities in the change tracker");
        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Unchanged, because: "we should have one entity in the change tracker in the unchanged state after finding it");
        session.ChangeTracker.Entries[0].State.Should().Be(EntityState.Unchanged, because: "we should have one entity in the change tracker in the unchanged state after finding it");
    }
}
